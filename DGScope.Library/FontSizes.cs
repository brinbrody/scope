namespace DGScope.Library
{
    public class FontSizes
    {
        private int dataBlocks = 1;
        private int lists = 1;
        private int dcb = 1;
        private int tools = 1;
        private int pos = 1;

        public int DataBlocks
        {
            get => dataBlocks;
            set
            {
                if (value >= 0 && value <= 5)
                    dataBlocks = value;
                else if (value < 0)
                    dataBlocks = 0;
                else if (value > 5)
                    dataBlocks = 5;
            }
        }
        public int Lists
        {
            get => lists;
            set
            {
                if (value >= 0 && value <= 5)
                    lists = value;
                else if (value < 0)
                    lists = 0;
                else if (value > 5)
                    lists = 5;
            }
        }
        public int DCB
        {
            get => dcb;
            set
            {
                if (value >= 0 && value <= 2)
                    dcb = value;
                else if (value < 0)
                    dcb = 0;
                else if (value > 2)
                    dcb = 2;
            }
        }
        public int POS
        {
            get => pos;
            set
            {
                if (value >= 0 && value <= 5)
                    pos = value;
                else if (value < 0)
                    pos = 0;
                else if (value > 5)
                    pos = 5;
            }
        }
    }
}