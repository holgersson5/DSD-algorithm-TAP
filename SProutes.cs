using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSDLogitSUE
{
    class SProutes
    {
        int spNum;
        int[,] spMatrix;
        Dictionary<int,Link> linkDic; // Alla länkar

        public SProutes(int spNum, int[,] spMatrix, Dictionary<int, Link> linkDic)
        {
            this.spNum = spNum;
            this.spMatrix = spMatrix;
            this.linkDic = linkDic; // Alla länkar
        }
        public SProutes()
        { // Default om inga värden specificeras
           


        }

        public int SPNum { get { return spNum; } set { spNum = value; } }
        public int[,] SP { get { return spMatrix; } set { spMatrix = value; } }
        public Dictionary<int, Link> LinkDic { get { return linkDic; } set { linkDic = value; } }

        public void setSP(int[,] SetSP) // 
        {
            spMatrix = new int[SetSP.GetLength(0), SetSP.GetLength(1)];
            for (int i = 0; i < SetSP.GetLength(0); i++)
            {
                for (int j = 0; j < SetSP.GetLength(1); j++)
                {
                    spMatrix[i, j] = SetSP[i, j];
                }
            }

           // return spMatrix;
        }



        public double BackTrack(int origin, int dest, double flow)
        {
            int temp = dest;
            double Gctemp = 0;
            while (true)
            {
                int linkid = spMatrix[origin, temp];
                if (linkid == 0)
                {
                    break;
                } 
                Link templink = new Link();
                templink = linkDic[linkid];
                Gctemp = Gctemp + templink.getVdTime(flow);
                temp = templink.FromNode;
            }
            return Gctemp;
        }
        public int CheckRoute(int[,] spCheck, int origin, int dest)
        {
            int temp = dest;
            int tempNumber = -1;
            while (true)
            {
                int linkid = spMatrix[origin, temp];
                int checkid = spCheck[origin, temp];
                if (linkid != checkid)
                {
                    tempNumber = 1;// ny rutt
                    break;
                }
                if (linkid == 0)
                {
                    tempNumber = -1; // Samma rutt
                    break;
                }
                Link templink = new Link();
                templink = linkDic[linkid];
                temp = templink.FromNode;
            }
            return tempNumber;

        }
        public double getCF(int[,] spCheck, int origin, int dest)
        {
            int temp1 = dest;
            int temp2 = dest;
            double l1 = 0, l12 = 0, l2 = 0;
            double cf = 0;        
            List<int> list1 = new List<int>();
            List<int> list2 = new List<int>();
            while (true)
            {
                int linkid = spMatrix[origin, temp1];
                int checkid = spCheck[origin, temp2];
                if (linkid == 0 && checkid == 0) // Måste bli 0 för båda rutter
                {
                    break;
                }         
                Link t1 = new Link(); 
                Link t2 = new Link();
                if (linkid > 0)
                {
                    t1 = linkDic[linkid];
                    l1 = l1 + t1.Length; // kan crasha vid mängd länkar i rutter // Ändra för -1
                    temp1 = t1.FromNode;
                    list1.Add(linkid);
                }
                if (checkid > 0)
                {
                    t2 = linkDic[checkid];
                    l2 = l2 + t2.Length;
                    temp2 = t2.FromNode;
                    list2.Add(checkid);
                }             
            }
            // Jämför listor//
            List<int> Same = new List<int>();
            Same = list1.Intersect(list2).ToList();
            foreach (var samelink in Same)
            {
                Link t12 = new Link();
                t12 = linkDic[samelink];
                l12 = l12 + t12.Length;
            }
            //Ekvation för cf 
            cf = l12 / (Math.Sqrt(l1) * Math.Sqrt(l2));
            return cf;
        }

        public List<int> getLinks(int origin, int dest)
        {
            int temp = dest;
            List<int> linkID = new List<int>();
            while (true)
            {
                int linkid = spMatrix[origin, temp];
                if (linkid == 0) break;
                Link templink = new Link();
                templink = linkDic[linkid];
                linkID.Add(linkid);
                temp = templink.FromNode;
            }
            return linkID;
        }

    }
}
