namespace DGScope.Library
{
    public class BrightnessSettings
    {
        private int dcb = 100;
        private int bkc = 100;
        private int mpa = 100;
        private int mpb = 100;
        private int fdb = 100;
        private int lst = 100;
        private int pos = 100;
        private int ldb = 100;
        private int oth = 100;
        private int tls = 100;
        private int rr = 100;
        private int cmp = 100;
        private int bcn = 100;
        private int pri = 100;
        private int hst = 100;
        private int wx = 100;
        private int wxc = 100;

        public int DCB
        {
            get => dcb;
            set
            {
                if (value >= 25 && value <= 100)
                    dcb = value;
                else if (value > 100)
                    dcb = 100;
                else if (value < 25)
                    dcb = 25;
            }
        }
        public int BKC
        {
            get => bkc;
            set
            {
                if (value <= 100)
                    bkc = value;
                else if (value > 100)
                    bkc = 100;
            }
        }
        public int MPA
        {
            get => mpa;
            set
            {
                if (value >= 5 && value <= 100)
                    mpa = value;
                else if (value > 100)
                    mpa = 100;
            }
        }
        public int MPB
        {
            get => mpb;
            set
            {
                if (value >= 5 && value <= 100)
                    mpb = value;
                else if (value > 100)
                    mpb = 100;
            }
        }
        public int FDB
        {
            get => fdb;
            set
            {
                if (value >= 5 && value <= 100)
                    fdb = value;
                else if (value < 5)
                    fdb = 0;
                else if (value > 100)
                    fdb = 100;
            }
        }
        public int LST
        {
            get => lst;
            set
            {
                if (value >= 25 && value <= 100)
                    lst = value;
                else if (value > 100)
                    lst = 100;
                else if (value < 25)
                    lst = 25;
            }
        }
        public int POS
        {
            get => pos;
            set
            {
                if (value >= 5 && value <= 100)
                    pos = value;
                else if (value < 5)
                    pos = 0;
                else if (value > 100)
                    pos = 100;
            }
        }
        public int LDB
        {
            get => ldb;
            set
            {
                if (value >= 5 && value <= 100)
                    ldb = value;
                else if (value < 5)
                    ldb = 0;
                else if (value > 100)
                    ldb = 100;
            }
        }
        public int OTH
        {
            get => oth;
            set
            {
                if (value >= 5 && value <= 100)
                    oth = value;
                else if (value < 5)
                    oth = 0;
                else if (value > 100)
                    oth = 100;
            }
        }

        public int TLS
        {
            get => tls;
            set
            {
                if (value >= 5 && value <= 100)
                    tls = value;
                else if (value < 5)
                    tls = 0;
                else if (value > 100)
                    tls = 100;
            }
        }
        public int RR
        {
            get => rr;
            set
            {
                if (value >= 5 && value <= 100)
                    rr = value;
                else if (value < 5)
                    rr = 0;
                else if (value > 100)
                    rr = 100;
            }
        }
        public int CMP
        {
            get => cmp;
            set
            {
                if (value >= 5 && value <= 100)
                    cmp = value;
                else if (value < 5)
                    cmp = 0;
                else if (value > 100)
                    cmp = 100;
            }
        }
        public int BCN
        {
            get => bcn;
            set
            {
                if (value >= 5 && value <= 100)
                    bcn = value;
                else if (value < 5)
                    bcn = 0;
                else if (value > 100)
                    bcn = 100;
            }
        }
        public int PRI
        {
            get => pri;
            set
            {
                if (value >= 5 && value <= 100)
                    pri = value;
                else if (value < 5)
                    pri = 0;
                else if (value > 100)
                    pri = 100;
            }
        }
        public int HST
        {
            get => hst;
            set
            {
                if (value >= 5 && value <= 100)
                    hst = value;
                else if (value < 5)
                    hst = 0;
                else if (value > 100)
                    hst = 100;
            }
        }
        public int WX
        {
            get => wx;
            set
            {
                if (value >= 5 && value <= 100)
                    wx = value;
                else if (value > 100)
                    wx = 100;
            }
        }
        public int WXC
        {
            get => wxc;
            set
            {
                if (value >= 5 && value <= 100)
                    wxc = value;
                else if (value > 100)
                    wxc = 100;
            }
        }
    }
}
