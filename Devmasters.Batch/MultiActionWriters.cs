namespace Devmasters.Batch
{
    public class MultiProgressWriter
    {
        System.Action<ActionProgressData>[] actionProgressFunctions = null;
        public MultiProgressWriter(params System.Action<ActionProgressData>[] actionProgressFunctions)
        {
            this.actionProgressFunctions = actionProgressFunctions;
        }

        public void ProgressWriter(ActionProgressData data)
        {
            if (actionProgressFunctions != null)
            {
                foreach (var w in actionProgressFunctions)
                {
                    if (w != null)
                        w(data);
                }
            }
        }

    }

    public class MultiOutputWriter
    {
        System.Action<string>[] actionOutputFunctions = null;
        public MultiOutputWriter(params System.Action<string>[] actionOutputFunctions)
        {
            this.actionOutputFunctions = actionOutputFunctions;
        }

        public void OutputWriter(string data)
        {
            if (actionOutputFunctions != null)
            {
                foreach (var w in actionOutputFunctions)
                {
                    if (w == null)
                        w(data);
                }
            }
        }

    }
}
