using solum.core.dataprocess.activities;
using solum.core.dataprocess.entries;
using solum.core.dataprocess.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace solum.core.dataprocess
{
    public class DataProcess : Process, IDataProcess
    {
        public static DataProcess Do<T>(Action<T> action)
        {
            var process = new DataProcess();
            var activity = new ActionActivity<T>(process, action);
            process.AddActivity(activity);

            return process;
        }
        public static DataSource<T> With<T>(IEnumerable<T> data)
        {
            var process = new DataProcess();
            var source = new DataSource<T>(process, data);
            process.AddSource(source);

            return source;
        }

        public DataProcess()
        {
            this.m_next_id = 0;
            this.m_sources = new List<IDataSource>();
            this.m_rootActivities = new List<IDataActivity>();
            this.m_activityLinks = new Dictionary<IDataActivity, List<IDataActivity>>();
            this.m_activityChain = new List<Tuple<IDataActivity, IDataActivity>>();
        }

        #region Events
        public event EventHandler OnStarted;
        public event EventHandler OnFinished;
        public event EventHandler<IDataEntry> OnEntryProcessed;
        void Started()
        {
            if (OnStarted != null)
                OnStarted(this, null);
        }
        void Finished()
        {
            if (OnFinished != null)
                OnFinished(this, null);
        }
        void EntryProcessed(IDataEntry entry)
        {
            if (OnEntryProcessed != null)
                OnEntryProcessed(this, entry);
        }
        #endregion

        #region Private Members
        long m_next_id;
        List<IDataSource> m_sources;
        List<IDataActivity> m_rootActivities;
        
        BroadcastBlock<IDataEntry> m_sourceBlock;
        ActionBlock<IDataEntry> m_completionBlock;

        List<Tuple<IDataActivity, IDataActivity>> m_activityChain;
        Dictionary<IDataActivity, List<IDataActivity>> m_activityLinks;
        #endregion

        public void AddSource<TInput>(DataSource<TInput> source)
        {
            this.OnStarted += (_, __) =>
            {
                Log.Debug("Reading sources...");
                foreach (var data in source.Read())
                {
                    ProcessData(data);
                }
            };

            m_sources.Add(source);
        }

        public void AddActivity<T>(DataActivity<T, T> activity)
        {
            m_rootActivities.Add(activity);
        }
        public void AddActivity<TInput>(DataSource<TInput> source, IDataActivity activity)
        {
            // ** Check if this activity is directly fed from the source
            m_rootActivities.Add(activity);
        }
        public void AddActivity(IDataActivity parent, IDataActivity child)
        {
            // ** Check if this activity is directly fed from the source
            m_activityChain.Add(Tuple.Create(parent, child));
        }
        public void LinkActivity(IDataActivity activity, params IDataActivity[] addLinks)
        {
            if (!m_activityLinks.ContainsKey(activity))
                m_activityLinks.Add(activity, new List<IDataActivity>());

            var links = m_activityLinks[activity];
            foreach (var link in addLinks)
            {
                if (links.Contains(link))
                {
                    Log.Warning("Ignoring duplicate link.");
                    continue;
                }

                links.Add(link);
            }
        }

        public OutputEntry<TIn, TOut> CreateEntry<TIn, TOut>(DataEntry<TIn> input, TOut output)
        {
            var id = NextEntryId();
            var entry = new OutputEntry<TIn, TOut>(input, id, output);

            return entry;
        }
        public override void Run()
        {
            Log.Debug("Starting processing...");

            // ** Track every block that needs to complete in the graph
            // var allBlocks = new List<IDataflowBlock>();            

            // ** Create a block to receive all data sent for processing
            m_sourceBlock = new BroadcastBlock<IDataEntry>(entry => entry); // TODO: Call clone method                        

            // ** Create a completion block that marks each data entry as complete as it happens
            m_completionBlock = new ActionBlock<IDataEntry>(entry => { EntryProcessed(entry); });

            // ** Build blocks for each activity chain
            var processBlocks = new List<IDataflowBlock>();

            // ** Chain any activities together
            var tailBlocks = new List<ISourceBlock<IDataEntry>>();
            foreach (var activity in m_rootActivities)
            {
                buildActivityChain(activity, m_sourceBlock, tailBlocks, processBlocks);
            }

            // ** Link all tails to the completion block
            foreach (var tail in tailBlocks)
            {
                tail.LinkTo(m_completionBlock, new DataflowLinkOptions()
                {
                    // PropagateCompletion = true
                });
            }

            // ** Ensure all activities are completed
            //m_sourceBlock.Completion.ContinueWith(_ =>
            //{
            //    activityBlocks.ForEach(a => a.Complete());
            //});

            // ** Notify everyone we have started
            Started();


            Log.Debug("Waiting for processing to finish...");

            // ** Send signal from the source block that we are finished.
            m_sourceBlock.Complete();
            m_sourceBlock.Completion.Wait();

            // Wait for all the blocks to complete
            var activitiesCompleted = processBlocks.Select(a => a.Completion).ToArray();

            //Thread.Sleep(5000);

            //Task.WaitAll(activitiesCompleted);
            Task.WaitAll(activitiesCompleted);

            // ** Wait for the final signal from the completion block
            m_completionBlock.Complete();
            m_completionBlock.Completion.Wait();

            // ** Notify everyone that we have finished
            Finished();
        }
        public void ProcessData<T>(T data)
        {
            var id = NextEntryId();
            var entry = new DataEntry<T>(id, data);
            PostEntry(entry);
        }
        protected void PostEntry(IDataEntry entry)
        {
            m_sourceBlock.Post(entry);
        }
        protected OutputEntry<TInput, TOutput> ProcessWithActivity<TInput, TOutput>(DataActivity<TInput, TOutput> activity, DataEntry<TInput> entry)
        {
            var output = activity.ProcessEntry(entry);
            return output;
        }

        public long NextEntryId()
        {
            var id = Interlocked.Increment(ref m_next_id);
            return id;
        }

        private void buildActivityChain(IDataActivity activity, ISourceBlock<IDataEntry> head, List<ISourceBlock<IDataEntry>> tails, List<IDataflowBlock> all)
        {
            // ** Create an activity block for the activity            
            var activityBlock = new TransformBlock<IDataEntry, IDataEntry>(entry => activity.ProcessEntry(entry), new ExecutionDataflowBlockOptions()
            {
                SingleProducerConstrained = true
            });
            all.Add(activityBlock);

            // ** Link activity to source
            head.LinkTo(activityBlock, new DataflowLinkOptions()
            {
                PropagateCompletion = true
            });

            // ** Process Siblings
            var siblings = m_activityChain.Where(t => t.Item1 == activity).Select(t => t.Item2).ToList();
            var hasSiblings = siblings.Count > 0;            
            foreach (var sibling in siblings)
            {
                // ** Chain all siblings to the same head
                buildActivityChain(sibling, head, tails, all);
            }

            // ** Process Children/Links (THEN)
            var hasChildren = (m_activityLinks.ContainsKey(activity) 
                               && m_activityLinks[activity].Count > 0);

            if (hasChildren)
            {
                foreach (var linkedActivity in m_activityLinks[activity])
                {
                    // ** Recursively link to any other blocks
                    buildActivityChain(linkedActivity, activityBlock, tails, all);
                }
            }
            else
            {
                // We've reached the tail end of the chain tail end of the chain
                tails.Add(activityBlock);
            }
        }
    }
}
