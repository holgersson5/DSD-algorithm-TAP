using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using MathNet;
using MathNet.Numerics;
using MathNet.Numerics.Integration;


namespace DSDLogitSUE
{
    class Program
    {
        private const int Infinity = int.MaxValue;
        private int n;
        private int[] pi;
        private Algorithm algo;
        private List<int> S;
        private List<Node> nodeList;
        private List<Link> linkList;
        private Dictionary<int, Link> LinkDic;
        private long CPUTime; 
        

        private int IndexOD = 0;
        private List<SProutes> SPList;
        SProutes tSP1 = new SProutes();
        private List<List<double>> OD;
        private List<List<double>>[] GcList;
        private List<double>[] rhFlow;
        private List<double>[] raFlow;
        private int r = 0;
        private double Step = 0;
        private int IndexLink = 1;

        private int NumOrigins;
        private int[,] SPMatrix;
        private double[,] xFlow;
        private double[,] yFlow;
        double[] traveltimeLink;
        double[] linkDirection;

        private int Link_Atgard = 1;
        private int testlinkID = 1;
        private List<int> o = new List<int>();
        private List<int> d1 = new List<int>();
        private List<double> rf = new List<double>();
        private List<int> rn = new List<int>();
        private int testorigin = 1;
        private int testdest = 9;
        private List<int> SaveLink = new List<int>();

        private double beta; 
        private double gamma;  

        private int Breakinner = 0;
        private int breakOut = 0;
        private bool[] FoundNew;
        private int k = 0;
        private int InnerNr = 1;

        private double[] UBD;
        private double[] LBD;

        private int[] n2i = new int[100000000];
        private int[,] TransOD;
        private int[,] TransLink;

        double[] x;
        double[] y;

        private double Scale; 

        // Konvergens // 
        private int Nriter = 50; // Total number of iterations
        private int MaxIter = 15; // Line search interations        
        private int MaxInner = 4; // RMP iterations MaxInner*k 
        private double error;

        private bool Kymlinge;
        private bool UE;
        private bool AltCF;
        private bool Medium = false;
       
        static void Main(string[] args)
        {

            // change depending network beroende på nätverk // 
            string SNode = "SmallNode.txt";
            string Sllink = "SmallLink1.txt"; // Byt SmallLink1Rev eller SmallLink1
            string SOD = "SmallOD.txt";

            string MNode = "MediumNode.txt";
            string MLink = "MediumLink2.txt"; // Kan ändras //
            string MOD = "MediumOD.txt";
            // Stockholm NV
            string LNode = "NodeSTHLM.txt";
            string LLink = "LinksSTHLM.txt";
            string LUALink = "LinksUASTHLM.txt";
            string LKymlingeLink = "LinksKymlingeSTHLM.txt";
            string LVasastanLink = "LinksSTHLMvasastan.txt";
            string LKymlingeJA = "LinksKymlingeJA.txt";
            string LOD = "OdSTHLM.txt";
            // Stor-Stockholm
            string XLNode = "NodeStorSTHLM.txt";
            string XLLink = "LinksStorSTHLM.txt";
            string XLOD = "ODStorSTHLM.txt";

            Program p = new Program();
            p.UE = true; // True if DSD-UE // False if DSD-CLogit //
            p.Medium = false; // True om medium körs // False annars //
            p.Scale = 1.0; // Scaling the demand
            p.error = 0.00001; //(0.00001 = 10^-5) // (UBD-LBD)/UBD //
            p.AltCF = true; // True om alternativ CF false om "Correct"

            if (p.Medium == true)
            {
                p.Link_Atgard = 5;
                p.ReadInput(MNode, MLink, MOD);
            }
            else 
            {
                p.ReadInput(LNode, LLink, LOD);
                p.Link_Atgard = 500; // 1018 Kymlinge // 500 VD-Valhallavägen // 424 Vasastaden // 
                p.testlinkID = 1919; // Vilka OD-par som har flöde på länken.
                p.testorigin = 101910;  // OD-pars länkflöde  // 
                p.testdest = 100020;
                p.Kymlinge = true;  // False om Jämförelse behövs //
            }
            if (p.UE == true)
            {
                p.gamma = 0;
                p.beta = 1;
            }
            else
            {
                // SUE parametrar //
                p.gamma = 1.0;  // Vikten av CF Faktorn //
                p.beta = 1.0; // Vikten av Logit spridningen //
            }           
            p.StartDSD();
            Console.Read();
        }

        private void StartDSD()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            int n = nodeList.Count();
            SPMatrix = new int[NumOrigins+1, n]; //(114*1021)
            algo = new Algorithm();
            int oldorigin = -1;
            xFlow = new double[linkList.Count(), Nriter];
            yFlow = new double[linkList.Count(), Nriter];
            traveltimeLink = new double[linkList.Count()];
            linkDirection = new double[linkList.Count()];
            UBD = new double[Nriter];
            LBD = new double[Nriter];
            FoundNew = new bool[Nriter]; // Deafault false 
            FoundNew[0] = true; FoundNew[1] = true;
            TransLink = new int[n, n];

            x = new double[linkList.Count()];
            y = new double[linkList.Count()];

            GcList = new List<List<double>>[OD.Count()];
            rhFlow = new List<double>[OD.Count()];
            raFlow = new List<double>[OD.Count()];
            for (int i = 0; i < OD.Count(); i++)
            {
                GcList[i] = new List<List<double>>();
                rhFlow[i] = new List<double>();
                raFlow[i] = new List<double>();
            }
            TransOD = new int[NumOrigins+1, n];
            SPList = new List<SProutes>();
            LinkDic = new Dictionary<int, Link>();
            foreach (var link in linkList)
            {
                LinkDic.Add(link.Id, link);
                TranslateLink(link.FromNode, link.ToNode);
            }
            var starttime = watch.ElapsedMilliseconds;
            Console.WriteLine("Initiering klar: " + starttime / 1000 + " sek " + " SP startar ");
            ////// Initialization Step 0 /////
            foreach (var od in OD)
            {
                int origin = Convert.ToInt32(od[0]); int dest = Convert.ToInt32(od[1]); double demand = Convert.ToDouble(od[2]);              
                TranslateOD(origin, dest); IndexOD++;                
                if (origin != oldorigin) 
                {
                    S = algo.Dijkstra(ref pi, ref nodeList, origin);
                    if (S != null)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            if (pi[i] != -1)
                            {
                                Link templink1 = new Link();
                                templink1 = LinkDic[LinkTranslate(nodeList[pi[i]].Id, nodeList[i].Id)];
                                SPMatrix[origin, i] = templink1.Id;
                            }
                            else if (origin == i)
                            {
                                SPMatrix[origin, i] = 0; // koppling till sig själv
                            }
                            else
                            {
                                SPMatrix[origin, i] = -1; // ingen koppling
                            }
                        }
                    } // SPMatrix klar för en Origin
                }
                oldorigin = origin; // spara gamla origin // 
            } // Next OD pair 
            SProutes tSP = new SProutes(); // ny möjlig SP-träd
            tSP.SPNum = 1;
            tSP.setSP(SPMatrix);
            tSP.LinkDic = LinkDic;
            SPList.Add(tSP);
            var SP0 = watch.ElapsedMilliseconds;
            Console.WriteLine("SP 0 Klar: " + SP0 / 1000 + " sek ");
            foreach (var od in OD)
            {
                //loop över iteration
                int origin = Convert.ToInt32(od[0]); int dest = Convert.ToInt32(od[1]); double demand = Convert.ToDouble(od[2]);
                raFlow[ODTranslate(origin, dest)].Add(demand);
                rhFlow[ODTranslate(origin, dest)].Add(0);                                                            
                List<int> linkID = new List<int>();
                linkID = SPList[0].getLinks(origin, dest);              
                foreach (var link in linkID)
                    {
                        xFlow[(link - 1), 0] = xFlow[(link - 1), 0] + raFlow[ODTranslate(origin, dest)][0]; // Uppdatera länkflöden 
                    } 
            }
            foreach (var od in OD)
            {
                int origin = Convert.ToInt32(od[0]); int dest = Convert.ToInt32(od[1]); List<int> linkID = new List<int>();               
                linkID = SPList[0].getLinks(origin, dest);
                double Gctemp = 0;
                foreach (var link in linkID)
                {
                    Link temp = new Link();
                    temp = LinkDic[link];
                    Gctemp = Gctemp + temp.getVdTime(xFlow[(link - 1), 0]);
                }
                GcList[ODTranslate(origin, dest)].Add(new List<double> { Gctemp, 0, 0 }); // ( gc,cf,index)
            }

            LBD[0] = -Infinity;
                                    
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Yttre loop Startar: " + elapsedMs/1000 + " sek ");
            while (k < Nriter) // Yttre loop k //    ((UBD[k] - LBD[k]) / UBD[k]) > error
            {
                /// Restricted master problem phase /////
                int inner = 0; Breakinner = 0;
                InnerNr = MaxInner * 10*k;
                while (inner < InnerNr && k!=0) // Inre loop //
                {
                    var Ms = watch.ElapsedMilliseconds;
                    Console.WriteLine("Start inner: " + inner + " tid " + Ms / 1000 + " sek ");
                    double innerLBD=0,innerUBD=0;
                    foreach (var od in OD) 
                    {
                        int origin = Convert.ToInt32(od[0]); int dest = Convert.ToInt32(od[1]); double demand = Convert.ToDouble(od[2]);                                            
                        List<List<double>> tGcList = new List<List<double>>();
                        r = 0;
                        foreach (var line in GcList[ODTranslate(origin, dest)])
                        {
                            int SPindex = Convert.ToInt32(line[2]);
                            double oldcf = line[1];
                            List<int> linkID = new List<int>();
                            raFlow[ODTranslate(origin, dest)][r] =  raFlow[ODTranslate(origin, dest)][r] + Step * (rhFlow[ODTranslate(origin, dest)][r]-raFlow[ODTranslate(origin, dest)][r]);
                          
                            linkID = SPList[SPindex].getLinks(origin, dest);
                            double Gctemp = 0;
                            foreach (var link in linkID)
                            {
                                Link temp = new Link();
                                temp = LinkDic[link];
                                Gctemp= Gctemp + temp.getVdTime(xFlow[(link - 1), k]);
                            }
                            tGcList.Add(new List<double> { Gctemp, oldcf, SPindex });                          
                            r++;
                        }
                        GcList[ODTranslate(origin, dest)] = tGcList; // Update gc flow all previos unique routes
                        bool checkSame = false; int sameAs = -2;                        
                        if (inner == 0) // Endast vid första inre loopen
                        {
                            for (int i = 0; i < k; i++)
                            {
                                sameAs = SPList[k].CheckRoute(SPList[i].SP, origin, dest); // ny möjlig rutt kollas mot alla tidigare träd.                          
                                // ny rutt sameAs==1
                                if (sameAs == -1) // Samma rutt
                                {
                                    checkSame = true; break;                                  
                                }
                            } // Alla unika rutter körda 
                            if ((checkSame != true)) // Uppdatera ALLA cf faktorer endast första inre loop
                            {
                                //Lägg till ny
                                double Sumcf = 0; double cf = 0;                             
                                r = 0;
                                foreach (var line in GcList[ODTranslate(origin, dest)]) // Uppdatera cf för alla rutter
                                {
                                    int SPindex = Convert.ToInt32(line[2]);
                                    cf = SPList[k].getCF(SPList[SPindex].SP, origin, dest);
                                    Sumcf = Sumcf + cf;
                                    if (cf != 0)
                                    {
                                        if (line[1] != 0)
                                        {
                                            if (AltCF == false) GcList[ODTranslate(origin, dest)][r][1] = gamma * Math.Log(cf + Math.Exp(line[1] / gamma));
                                        }
                                        else
                                        {
                                            if(AltCF == false) GcList[ODTranslate(origin, dest)][r][1] = gamma * Math.Log(1 + cf);
                                        }
                                    }
                                    r++;
                                }
                                if (Sumcf != 0)
                                {
                                    Sumcf = gamma * Math.Log(1 + Sumcf);   
                                }
                                else
                                {
                                    Sumcf = gamma * Math.Log(1);
                                }
                                raFlow[ODTranslate(origin, dest)].Add(0.0);
                                rhFlow[ODTranslate(origin, dest)].Add(0.0);
                                List<int> linkID = new List<int>();
                                linkID = SPList[k].getLinks(origin, dest);
                                double Gctemp = 0;
                                foreach (var link in linkID)
                                {
                                    Link temp = new Link();
                                    temp = LinkDic[link];
                                    Gctemp = Gctemp + temp.getVdTime(xFlow[(link - 1), k]);
                                }
                                GcList[ODTranslate(origin, dest)].Add(new List<double> { Gctemp, Sumcf, k });                             
                                FoundNew[k] = true;
                            }                 
                        }                       
                        List<double> P = Logit(GcList[ODTranslate(origin, dest)]);
                        r = 0; double prevlambda = 0.0;
                        int highlamda = 0;
                        foreach (var lamda in P)
                        {
                            rhFlow[ODTranslate(origin, dest)][r] = lamda * demand;
                            if (UE == true)
                            {
                                rhFlow[ODTranslate(origin, dest)][r] = 0;
                                if (lamda > prevlambda)
                                {
                                    rhFlow[ODTranslate(origin, dest)][r] = 0;
                                    highlamda = r;
                                    prevlambda = lamda;
                                }  
                            }                                                     
                            r++;
                        }
                            if (UE == true) rhFlow[ODTranslate(origin, dest)][highlamda] = demand;
                            r = 0; // gå över till länkflöden //
                            foreach (var line in GcList[ODTranslate(origin, dest)])
                            {
                                double gc = line[0]; double cf = line[1]; int SPindex = Convert.ToInt32(line[2]);                                                              
                                List<int> linkID = new List<int>();
                                linkID = SPList[SPindex].getLinks(origin, dest);                               
                                foreach (var link in linkID)
                                {                                  
                                    x[(link - 1)] = x[(link - 1)] + raFlow[ODTranslate(origin, dest)][r]; // (p_opt*demand) tidigare lösning //
                                    y[(link - 1)] = y[(link - 1)] + rhFlow[ODTranslate(origin, dest)][r]; //(p*demand) 
                                   // Console.Write(link + ",");
                                }
                                if (UE == false)
                                {
                                    innerUBD = innerUBD + Zobj(raFlow[ODTranslate(origin, dest)][r], cf);
                                    innerLBD = innerLBD + Zobj(rhFlow[ODTranslate(origin, dest)][r], cf);                                    
                                }                                                                                         
                                r++;
                            }                       
                    } // Slut på OD foreach // 
                    for (int i = 0; i < linkList.Count(); i++)
                    {
                        Link templink = new Link();
                        templink = LinkDic[(i + 1)];
                        xFlow[i, k] = x[i]; yFlow[i, k] = y[i];
                        x[i] = 0; y[i] = 0; // Nollställ temporärt flöde // 
                        innerUBD = innerUBD + templink.getObjectiveValue(xFlow[i, k]);
                        if (UE == false)
                        {
                            innerLBD = innerLBD + templink.getObjectiveValue(yFlow[i, k]);
                        }
                        else
                        {
                            innerLBD = innerLBD + (templink.getVdTime(xFlow[i, k]) * (yFlow[i, k] - xFlow[i, k]));
                        }                        
                    }
                    double limit=0;
                    if (UE == false)
                    {
                        limit = Math.Abs((innerLBD - innerUBD)/innerLBD);
                    }else
                    {                      
                        if (k > 0 )
                        {
                            LBD[k] = innerLBD; UBD[k] = innerUBD;
                            LBD[k] = LBD[k] + UBD[k];
                            LBD[k] = Math.Max(LBD[k], LBD[k - 1]);
                        }
                        limit = Math.Abs((UBD[k] - LBD[k]) / UBD[k]);
                    }
                    if (limit < error && (inner !=0))
                    {
                        Breakinner++;
                        if (Breakinner >= 2)
                        {
                            Console.WriteLine(" // Breaks inner loop  // ");
                             UBD[k] = innerUBD; Step = 0;
                             if (UE == false) LBD[k] = innerLBD;
                            break; 
                        }                     
                    }
                    // linjesökning //
                    int linjesok = 0;
                    double alfa1 = 0, alfa2 = 1, s = ((alfa1 + alfa2)) / 2.0;
                    double[] xTemp = new double[linkList.Count()]; double RDlink = 0, RDroute=0,raFlowtemp = 0; 
                    List<double> rdirection = new List<double>();
                    while (linjesok < MaxIter)
                    {   
                        List<double> z = new List<double>();
                        foreach (var od in OD)
                        {
                            int origin = Convert.ToInt32(od[0]); int dest = Convert.ToInt32(od[1]);                          
                            r=0;                           
                            foreach (var line in GcList[ODTranslate(origin, dest)])
                            {
                                int index = Convert.ToInt32(line[2]); double cf = line[1];                               
                                double rh = rhFlow[ODTranslate(origin, dest)][r];
                                double ra = raFlow[ODTranslate(origin, dest)][r];
                                if (linjesok == 0) rdirection.Add(rh - ra);

                                raFlowtemp = ra + s * (rh - ra);
                                z.Add(Zprim(raFlowtemp, cf)); // Route derivetive                                                                                                                        
                                r++;
                            }                           
                        }
                         for (int i = 0; i < linkList.Count(); i++)
                         {
                             xTemp[i] = xFlow[i, k] + s * (yFlow[i, k] - xFlow[i, k]);
                             if (linjesok == 0) linkDirection[i] = (yFlow[i, k] - xFlow[i, k]);
                             Link templink = new Link();
                             templink = LinkDic[(i + 1)];
                             traveltimeLink[i] = templink.getVdTime(xTemp[i]);
                         }
                         RDlink = traveltimeLink.Zip(linkDirection, (d1, d2) => d1 * d2).Sum();                       
                         RDroute = z.Zip(rdirection, (d1, d2) => d1 * d2).Sum();
                         if (UE == true) RDroute = 0;
                         double totRD = RDlink +RDroute;
                         if (totRD <= 0)
                         {
                             alfa1 = s;
                         }
                         else if (totRD > 0)
                         {
                             alfa2 = s;
                         }
                         s = (alfa1 + alfa2) / 2.0;
                         linjesok++;
                         if (linjesok == MaxIter)
                         {
                             for (int linkNr = 0; linkNr < linkList.Count(); linkNr++)
                             {
                                 xTemp[linkNr] = xFlow[linkNr, k] + s * (yFlow[linkNr, k] - xFlow[linkNr, k]);
                                 xFlow[linkNr, k] = xTemp[linkNr];
                             }
                             Step = s; 
                         }                        
                    }   
              
                    var Ms4 = watch.ElapsedMilliseconds;
                    Console.WriteLine("Linjesökning klar:" + Ms4 / 1000 + " sek " + " inre iter: " + inner + " Step " + Math.Round(Step, 5));
                    inner++;
                    if (inner == InnerNr)
                    {
                        LBD[k] = innerLBD; UBD[k] = innerUBD;
                        if (k > 0 && UE == true)
                        {
                                LBD[k] = LBD[k] + UBD[k];
                                LBD[k] = Math.Max(LBD[k], LBD[k - 1]);                           
                        }
                    }
                }                
                Console.WriteLine("k " + k + " LBD " + Math.Round(LBD[k], 2) + " UBD " + Math.Round(UBD[k], 2));
                
                //// Kolumngenerering  ///
                if (FoundNew[k] == false)
                {
                    Console.WriteLine("Found no new routes");
                    breakOut++;
                    if (breakOut >= 5)
                    {                       
                        foreach (var od in OD)
                        {
                            int origin = Convert.ToInt32(od[0]); int dest = Convert.ToInt32(od[1]);                            
                            r = 0;
                            foreach (var line in GcList[ODTranslate(origin, dest)])
                            {
                                raFlow[ODTranslate(origin, dest)][r] = raFlow[ODTranslate(origin, dest)][r] + Step * (rhFlow[ODTranslate(origin, dest)][r] - raFlow[ODTranslate(origin, dest)][r]);
                                r++;
                            }
                        }
                        CPUTime = watch.ElapsedMilliseconds/ 1000;
                        Console.WriteLine("Found no new routes in " + (breakOut+1) + " iter: Quit! ");
                        goto Quit;   
                    }                    
                }

                var nodde = watch.ElapsedMilliseconds;
                Console.WriteLine("Uppdaterar nodkostnad start: " + nodde / 1000 + " sek ");
                    //RE-Calculate new travel times nodelist.weights
                    foreach (var node in nodeList) // Update weights
                    {
                        int d = 0;
                        foreach (var adjecent in node.Adjacency)
                        {
                            Link templink2 = new Link();
                            templink2 = LinkDic[LinkTranslate(node.Id, adjecent.Id)];
                            node.Weights[d] = templink2.getVdTime(xFlow[(templink2.Id - 1), k]);
                            d++;
                        }
                    }
                int oldorigin1 = -1;
                var SP = watch.ElapsedMilliseconds;
                Console.WriteLine("Start ny SP baserat på uppdaterade reskostnader: " + SP / 1000 + " sek ");
                //Calculate SP-with new travel times for each origin                 
                foreach (var od in OD)
                {
                    int origin = Convert.ToInt32(od[0]); int dest = Convert.ToInt32(od[1]); double demand = Convert.ToDouble(od[2]);                  
                    if (origin != oldorigin1) // ny origin => ny SPMatrix
                    {
                        S = algo.Dijkstra(ref pi, ref nodeList, origin);
                        if (S != null)
                        {
                            for (int i = 0; i < n; i++)
                            {
                                if (pi[i] != -1)
                                {
                                    Link templink1 = new Link();
                                    templink1 = LinkDic[LinkTranslate(nodeList[pi[i]].Id, nodeList[i].Id)];
                                    SPMatrix[origin, i] = templink1.Id;
                                }
                                else if (origin == i)
                                {
                                    SPMatrix[origin, i] = 0; // koppling till sig själv
                                }
                                else
                                {
                                    SPMatrix[origin, i] = -1; // ingen koppling
                                }
                            }
                        } // SPMatrix klar för en Origin
                    }
                    oldorigin1 = origin; // spara gamla origin // 
                }// Next OD pair 
                var Ms1 = watch.ElapsedMilliseconds;
                Console.WriteLine();
                Console.WriteLine("iter: " + (k + 1) + " Tid: " + Ms1 / 1000 + " sek ");               
                k++;              
                SProutes tSP1 = new SProutes();
                tSP1.SPNum = k;
                tSP1.setSP(SPMatrix);
                tSP1.LinkDic = LinkDic;
                SPList.Add(tSP1);
                if (k == Nriter)
                {
                    CPUTime = Ms1 / 1000;
                    k--; break;
                }
                for (int i = 0; i < linkList.Count(); i++)
                {
                    xFlow[i, k] = xFlow[i, (k - 1)];
                }
            }// Slut while 
            Quit:
            WriteOutput();
        }
        private double Zobj(double Rflow, double cf)
        {
            double routeobj = 0;
            if (Rflow != 0)
            {
                routeobj = ((1/beta)*(Rflow * Math.Log(Rflow))) + Rflow * cf;
            }
            return routeobj;
        }
        private double Zprim(double Rflow, double cf)
        {
            double routeprim = 0;
            if (Rflow != 0)
            {
                routeprim = ((1 / beta) * (Math.Log(Rflow) + 1)) + cf;
            }
            return routeprim;
        }

        private List<double> Logit(List<List<double>> gcTemp)
        {
            List<double> share = new List<double>();
            double sum = 0; double bas = 0;
            double VarGCBeta = 0;
            List<double> varBeta = new List<double>();
            foreach (var line in gcTemp) // line[0] = gc line[1] = cf, line[2] = index
            {
                double gc = line[0]; double cf = line[1];               
                if (sum == 0)
                {
                    bas = gc + cf;
                }
                varBeta.Add((gc + cf));
            }
            double avg = varBeta.Average();
            double s = varBeta.Sum(d => Math.Pow(d - avg, 2));
            VarGCBeta = Math.Sqrt((s) / (varBeta.Count() - 1));            
            VarGCBeta = Math.Log(1 + VarGCBeta);
            //Console.WriteLine("VarBeta = " + Math.Round(VarGCBeta, 2));
            if (varBeta.Count() == 1 || VarGCBeta>1)
            {
                VarGCBeta = 1;
            }
            foreach (var line in gcTemp) // line[0] = gc line[1] = cf, line[2] = index
            {
                double gc = line[0];
                double cf = line[1];
                if (sum == 0)
                {
                    bas = gc + cf;
                }
                sum = sum + Math.Exp(-beta * ((gc + cf) - bas));
            }
            foreach (var line in gcTemp)
            {
                double gc = line[0];
                double cf = line[1];
                share.Add(Math.Exp(-beta * ((gc + cf) - bas)) / sum);
            }
            return share;
        }
        private void ReadInput(string nodetext, string linktext, string odtext)
        {
            //// NODE DATA /////
            nodeList = new List<Node>();
            int counter1 = 0; string line1;
            //double xCord, yCord;
            int Num = 0; char delimiter = ' ';
            System.IO.StreamReader fileNod = new System.IO.StreamReader("../../../Data/" + nodetext);
            while ((line1 = fileNod.ReadLine()) != null)
            {
                Node TestNode = new Node(Infinity, 0, 0, 0, "a");
                counter1++;
                if (counter1 > 6) // Eller ta bort övertext
                {
                    String[] substrings1 = line1.Split(delimiter);
                    substrings1 = substrings1.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    int sep1 = 0; // next line
                    foreach (var substring1 in substrings1)
                    {
                        if (sep1 == 0)
                        { // a
                            sep1++;
                        }
                        else if (sep1 == 1)
                        { // node
                            string nodeNr = substring1;
                            int nodNR = Int32.Parse(nodeNr);
                            n2i[Num] = nodNR;
                            TestNode.Id = Num;
                            Num++;
                            sep1++;
                        }
                        else if (sep1 == 2)
                        { // Om läsning kör om till Km koordinater

                            TestNode.Xcord = double.Parse(substring1, System.Globalization.CultureInfo.InvariantCulture); 
                            sep1++;
                        }
                        else if (sep1 == 3)
                        {
                            TestNode.Ycord = double.Parse(substring1, System.Globalization.CultureInfo.InvariantCulture);
                            sep1++;
                        }
                    }
                    nodeList.Add(TestNode);
                }
            }

            System.IO.StreamReader fileOD =
           new System.IO.StreamReader("../../../Data/" + odtext);
            /////// OD indata //////////
            string lineOD;
            int counter = 0; int origin = -1; int dest = 0;
            double ODdemand = 0.0; int oldOrigin = -1;
            OD = new List<List<double>>();
            while ((lineOD = fileOD.ReadLine()) != null)
            {
                counter++;
                if (counter > 4) // Eller ta bort övertext
                {
                    String[] substrings = lineOD.Split(delimiter, ':');

                    substrings = substrings.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    int sep = 0; // next line
                    foreach (var substring in substrings)
                    {
                        if (sep % 2 == 0)
                        {
                            // is even
                            if (sep == 0)
                            {// origin                               
                                origin = Int32.Parse(substring);
                                if (oldOrigin != origin)
                                {
                                    NumOrigins++; // räknar unika origins
                                }
                                oldOrigin = origin;
                            }
                            else
                            {
                                ODdemand = double.Parse(substring, System.Globalization.CultureInfo.InvariantCulture);
                                OD.Add(new List<double> { N2I(origin), N2I(dest), ODdemand*Scale });
                            }
                        }
                        else
                        { // is odd                                                          
                            dest = Int32.Parse(substring);
                        }
                        sep++;
                    }
                }
            }
            n = nodeList.Count;
            //// Länk DATA //// 
            string line2; int counter2 = 0;
            System.IO.StreamReader fileLink =
                 new System.IO.StreamReader("../../../Data/" + linktext);
            List<List<double>> FromToLength = new List<List<double>>();

            linkList = new List<Link>();
            int from = 0; int toNode = -3; double length = 0.0;
            int vd = -1; int ul2 = 100000; int ul3 = 900;
            double kf = 0;
            while ((line2 = fileLink.ReadLine()) != null)
            {
                counter2++;
                if (counter2 > 2) // Eller ta bort övertext
                {
                    Link templink = new Link();
                    String[] substrings2 = line2.Split(delimiter);
                    substrings2 = substrings2.Where(y => !string.IsNullOrEmpty(y)).ToArray();
                    int sep2 = 0; // next line
                    foreach (var substring2 in substrings2)
                    {
                        if (sep2 == 0)
                        { // a
                            sep2++;
                        }
                        else if (sep2 == 1)
                        {
                            from = Int32.Parse(substring2);
                            sep2++;
                        }
                        else if (sep2 == 2)
                        {
                            int to = Int32.Parse(substring2);
                            toNode = to;
                            sep2++;
                        }
                        else if (sep2 == 3)
                        {
                            length = Convert.ToDouble(substring2, System.Globalization.CultureInfo.InvariantCulture);
                            FromToLength.Add(new List<double> { N2I(from), N2I(toNode), length });
                            sep2++;
                        }
                        else if (sep2 == 4)
                        { // mode
                            sep2++;
                        }
                        else if (sep2 == 5)
                        {   //mode
                            sep2++;
                        }
                        else if (sep2 == 6)
                        {   //type
                            //kf = Int32.Parse(substring2);
                            kf = Convert.ToDouble(substring2, System.Globalization.CultureInfo.InvariantCulture);
                            sep2++;
                        }
                        else if (sep2 == 7)
                        {
                            vd = Int32.Parse(substring2);
                            sep2++;
                        }
                        else if (sep2 == 8)
                        {

                            //ul1 = Int32.Parse(substring2);
                            sep2++;
                        }
                        else if (sep2 == 9)
                        {
                            ul2 = Int32.Parse(substring2);
                            sep2++;
                        }
                        else if (sep2 == 10)
                        {

                            ul3 = Int32.Parse(substring2);
                            templink.Id = (counter2 - 1);
                            templink.FromNode = N2I(from);
                            templink.ToNode = N2I(toNode);
                            templink.Length = length;
                            templink.Kf = kf;
                            templink.Vdvalue = vd;
                            templink.Ul2 = ul2;
                            templink.Ul3 = ul3;

                            linkList.Add(templink);

                            sep2++;
                        }

                    }
                }
            }
           // List<Node> edges = new List<Node>();
            List<double> weights = new List<double>();
            int fromTemp = -2; int toTemp = -2;
            foreach (var line in FromToLength)
            {
                fromTemp = Convert.ToInt32(line[0]);
                toTemp = Convert.ToInt32(line[1]);
                double lengthTemp = line[2];
                Link temp = new Link();
                temp = linkList.Find(x => (x.FromNode == fromTemp) && (x.ToNode == toTemp));
                nodeList[fromTemp].Adjacency.Add(nodeList[toTemp]);
                nodeList[fromTemp].Weights.Add(temp.getVdTime(0));
            }
            foreach (var Node in nodeList)
            {
                Node.Name = Convert.ToString(Node.Id);  // ändra namn endast
            }

        }
        private int N2I(int isn)
        {
            int temp = 0;
            temp = Array.IndexOf(n2i, isn);
            return temp;
        }
        private int I2N(int nsi) 
        {
            int temp = 0;
            temp = n2i[nsi];
            return temp;
        }

        private int TranslateOD(int o, int d)
        {
            TransOD[o, d] = IndexOD;
            return IndexOD;

        }

        private int ODTranslate(int o, int d)
        {
            int temp = 0;
            temp = TransOD[o, d];
            return temp;
        }
        private int TranslateLink(int fromNode, int toNode)
        {
            TransLink[fromNode, toNode] = IndexLink;
            IndexLink++;
            return IndexLink;
        }

        private int LinkTranslate(int fromNode, int toNode)
        {
            int temp = 0;
            temp = TransLink[fromNode, toNode];
            return temp;
        }
        private void WriteOutput()
        {
            
            using (System.IO.StreamWriter OutputxFlow =
            new System.IO.StreamWriter("../../../../Output/OutputxFlow.txt"))
            {
                for (int i = 0; i <linkList.Count() ; i++)
                {
                    for (int j = 0; j <= k; j++)
                    {
                        OutputxFlow.Write(xFlow[i, j] + " ");
                    }
                    OutputxFlow.WriteLine();
                }
            }
            
            using (System.IO.StreamWriter OutputRouteFlowfile =
new System.IO.StreamWriter("../../../../Output/RouteFlowOut.txt"))
            {
                OutputRouteFlowfile.WriteLine("origin" + " dest" + " routeNr" + " raFlow" + " rhFlow" + " GC" + " CF" + " P");
                foreach (var od in OD)
                {
                    int origin = Convert.ToInt32(od[0]);
                    int dest = Convert.ToInt32(od[1]);
                    double demand = Convert.ToDouble(od[2]);
                    
                    int r = 0;
                    List<int> linkID = new List<int>();
                    foreach (var line in GcList[ODTranslate(origin, dest)])
                    {
                        int SPindex = Convert.ToInt32(line[2]);
                        linkID = SPList[SPindex].getLinks(origin, dest);
                        double Gctemp = 0; 
                        foreach (var link in linkID)
                        {                                                                            
                           Link temp = new Link();
                           temp = LinkDic[link];
                           Gctemp = Gctemp + temp.getVdTime(xFlow[(link - 1), k]);                         
                            if (((I2N(origin) == testorigin) && (I2N(dest) == testdest)))
                            {
                                x[(link - 1)] = x[(link - 1)] + raFlow[ODTranslate(origin, dest)][r];
                                y[(link - 1)] = y[(link - 1)] + rhFlow[ODTranslate(origin, dest)][r]; 
                                //o.Add(origin); d1.Add(dest); rn.Add(r + 1); rf.Add(raFlow[ODTranslate(origin, dest)][r]); 
                                if (SaveLink.Contains(link))
                                {
                                    // Finns redan i Lista
                                }
                                else
                                {
                                    SaveLink.Add(link);
                                }
                            }
                        }
                        OutputRouteFlowfile.WriteLine(I2N(origin) + " " + I2N(dest) + " " + (r + 1) + " " + Math.Round(raFlow[ODTranslate(origin, dest)][r], 4) + " " + Math.Round(rhFlow[ODTranslate(origin, dest)][r], 4) + " " + Math.Round(Gctemp, 4) + " " + Math.Round(line[1], 4) + " " + Math.Round((raFlow[ODTranslate(origin, dest)][r]) / demand, 4)); 
                        r++;
                    }
                }
            }
            using (System.IO.StreamWriter OutputLBDUBDfile =
             new System.IO.StreamWriter("../../../../Output/UBDLBDOut.txt"))
                      {
                          OutputLBDUBDfile.WriteLine("LBD " + "UBD");
                          for (int j = 0; j <=k; j++)
                          {
                              OutputLBDUBDfile.WriteLine(Math.Round(LBD[j], 2) +" " +Math.Round(UBD[j], 2) );
                          }
                      }
                      using (System.IO.StreamWriter OutputResult =
          new System.IO.StreamWriter("../../../../Output/Result.txt"))
                      {
                          OutputResult.WriteLine("Number of iterations: " + k + " CPU time: " + CPUTime);
                          OutputResult.WriteLine("origin " + " dest " + "length " +"xFlow " +"Traveltime " + "VehKm " + "VehTime " + " AvgSpeed" + " GenCost");
                          double  voth = (100.0/60.0);
                          for (int j = 0; j < linkList.Count(); j++)
                          {
                              Link temp = new Link();
                              temp = LinkDic[j + 1];
                              double vehkm = xFlow[j, k]*temp.Length;
                              double temp1 = (temp.Length * 1.8);
                              double vdtime = temp.getVdTime(xFlow[j, k]);
                              double traveltime = (vdtime-temp1)/voth;
                              double vehtime = xFlow[j,k]*(traveltime/60);
                              double speed = 0;
                              if(vehtime!=0)speed=vehkm/vehtime;
                              if (Kymlinge == false && (j+1) == 982) OutputResult.WriteLine(102563 + " " + 102624 + " " + 1.58 + " " + 0 + " " + 0 + " " + 0 + " " + 0 + " " + 0);
                              if (Kymlinge == false && (j+1) == 1017) OutputResult.WriteLine(102624 + " " + 102563 + " " + 1.58 + " " + 0 + " " + 0 + " " + 0 + " " + 0 + " " + 0);
                              OutputResult.WriteLine(I2N(temp.FromNode) + " " + I2N(temp.ToNode) + " " + temp.Length + " " + Math.Round(xFlow[j, k], 2) + " " + Math.Round(traveltime, 4) + " " + Math.Round(vehkm, 4) + " " + Math.Round(vehtime, 4) + " " + Math.Round(speed, 4) + " " + Math.Round(vdtime, 4));
                          }
                      }
            
                      using (System.IO.StreamWriter LinkRouteCheck =
                   new System.IO.StreamWriter("../../../../Output/LinkRouteCheck.txt"))
                      {
                          LinkRouteCheck.WriteLine("linkNumber " + testlinkID);
                          LinkRouteCheck.WriteLine("origin " + "dest " + "RouteNR " + " RouteFlow");
                          for (int i = 0; i < o.Count();i++)
                          {
                              LinkRouteCheck.WriteLine(o[i] + " " + d1[i] + " " + rn[i] + " " + rf[i]);
                          }
                      }

                      using (System.IO.StreamWriter ODrouteCheck =
                        new System.IO.StreamWriter("../../../../Output/ODrouteCheck.txt"))
                      {
                          ODrouteCheck.WriteLine("OD " + testorigin + " " + testdest);
                          ODrouteCheck.WriteLine("TotalLinkflow " + "ODLinkflow ");
                          for (int i = 0; i < linkList.Count(); i++)
                          {
                              if (SaveLink.Contains(i + 1))
                              {
                                  ODrouteCheck.WriteLine(xFlow[i, k] + " " + x[i]);
                              }
                              else
                              {
                                  ODrouteCheck.WriteLine(0 + " " + x[i]);
                              }
                          }
                      }

                      // Resultat fordonsarbete funktion av distans //
                     //Link_Atgard
                      Console.WriteLine();
                      Link atgard = new Link();
                      atgard = LinkDic[Link_Atgard];
                      int startnod = atgard.FromNode;                      
                      Node Startnode = new Node();
                      Startnode = nodeList[startnod];
                      List<List<int>> RadNodeId = new List<List<int>>();
                      List<List<int>> RadLinkId = new List<List<int>>();
                     // Console.WriteLine(" Startnode : " + Startnode.Id  + " From " + atgard.FromNode + " To " + atgard.ToNode + " X " + Startnode.Xcord + " Y " + Startnode.Ycord);

                      double tempradius = 0;
                      foreach (var node in nodeList)
                      {                         
                          tempradius = Math.Sqrt((Math.Pow(node.Xcord - Startnode.Xcord, 2) + Math.Pow(node.Ycord - Startnode.Ycord, 2)));
                         // Console.WriteLine("radius: " + tempradius + " nodeID " + node.Id);
                          tempradius = Math.Ceiling(tempradius);
                          int tradius = Convert.ToInt32(tempradius);
                          RadNodeId.Add(new List<int> { node.Id, tradius });

                      }
                      RadNodeId = RadNodeId.OrderBy(list => list[1]).ToList();
                      foreach (var node in RadNodeId)
                      {
                         // Console.WriteLine("nodeId: " + node[0] + " Radius: " + node[1]);
                          int fromnode = node[0]; int rad = node[1];                          
                          Node tempNode = new Node();
                          tempNode = nodeList[fromnode];
                          foreach (var adjecent in tempNode.Adjacency)
                          {
                              Link tempTO = new Link();
                              tempTO = LinkDic[LinkTranslate(fromnode, adjecent.Id)];
                              
                              RadLinkId.Add(new List<int> { tempTO.Id, rad }); // Kan addera länklängd eller vd // 
                          }
                      }
                      using (System.IO.StreamWriter OutputFkm =
                     new System.IO.StreamWriter("../../../../Output/OutputFkm.txt"))
                      {
                          OutputFkm.WriteLine("Fkm " + "Radie " + " Flow" + " linkID");
                          foreach (var line in RadLinkId)
                          {
                              int linkId = line[0];
                              Link l = new Link();
                              l = LinkDic[linkId];
                              double fkm = xFlow[l.Id - 1, k] * l.Length;
                              OutputFkm.WriteLine(fkm + " " + line[1] + " " + xFlow[l.Id - 1, k] + " " + l.Id);
                          }
                      }
                     
            
                          Console.WriteLine("Output finished: you may close the stupid program!");

        }

    }
}
