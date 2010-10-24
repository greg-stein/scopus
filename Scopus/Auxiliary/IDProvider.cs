using System;

namespace Scopus.Auxiliary
{
    public class IDProvider
    {
        private const int INITIAL_ID = -1;
        private readonly int max = -1;
        private readonly int start = INITIAL_ID;
        private int mCurrentInt;

        public IDProvider()
        {
            Reset();
        }

        public IDProvider(int start)
        {
            this.start = start - 1;
            mCurrentInt = this.start;
        }

        public IDProvider(int start, int max) : this(start)
        {
            this.max = max;
        }

        public bool RollingCounter { get; set; }

        internal void Reset()
        {
            mCurrentInt = INITIAL_ID;
        }

        internal int GetNext()
        {
            if (max != -1 && mCurrentInt >= max)
            {
                if (RollingCounter)
                    mCurrentInt = start;
                else
                    throw new InvalidOperationException("End reached and no rolling counter defined.");
            }

            return ++mCurrentInt;
        }

        internal int GetCurrent()
        {
            if (mCurrentInt == start)
                throw new InvalidOperationException("Attempt to call GetCurrent before call to GetNext.");
            return mCurrentInt;
        }
    }
}