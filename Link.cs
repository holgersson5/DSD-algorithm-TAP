using MathNet.Numerics.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSDLogitSUE
{
    class Link
    {
        int id, fromNode, toNode;
        double length;
        int vdValue;
        double kf;
        int ul2, ul3;


        public Link(int id, int fromNode, int toNode, double length, int kf, int vdValue, int ul2, int ul3)
        {

            this.id = id;
            this.fromNode = fromNode;
            this.toNode = toNode;
            this.length = length;
            this.kf = kf;
            this.vdValue = vdValue;
            this.ul2 = ul2;
            this.ul3 = ul3;

        }
        public Link()
        { // Default om inga värden specificeras
            id = 0;
            fromNode = -1;
            toNode = -1;
            length = 1;
            kf = 0;
            vdValue = -1;
            ul2 = 10000;
            ul3 = 900;
        }

        public int Id { get { return id - 1; } set { id = value; } }
        public int FromNode { get { return fromNode; } set { fromNode = value; } }
        public int ToNode { get { return toNode; } set { toNode = value; } }
        public double Length { get { return length; } set { length = value; } }
        public double Kf { get { return kf; } set { kf = value; } }
        public int Vdvalue { get { return vdValue; } set { vdValue = value; } }
        public int Ul2 { get { return ul2; } set { ul2 = value; } }
        public int Ul3 { get { return ul3; } set { ul3 = value; } }


        public double getVdTime(double Qflow)
        {

            bool TU71 = true; // False if TU06

            double c1 = 0, c0 = 0, c2 = 0, c5 = 0, c6 = 0, c3 = 0;
            double f1 = 0, f2 = 0, f3 = 0, f4 = 0, f5 = 0, f6 = 0, f7 = 0;
            double f8 = 0, f9 = 0, f10 = 0, f11 = 0, f12 = 0, f13 = 0, f14 = 0, f15 = 0, f16 = 0, f17 = 0, f18 = 0;
            double a1 = 0, a2 = 0, a3 = 0, a4 = 0, a5 = 0, a6 = 0;
            double talj = 0, namn = 0, hel = 0, MINSTA = 0;
            double UTILX = 0.95;
            int c4 = 0;
            double t = 0;
            int Qbreak = 0;
            double Qadd = 0; //oklart vad dena tillför
            Convert.ToDouble(kf);
            double X = (Qflow + Qadd) / kf; //flöde per körfält
            double voth = (100.0 / 60.0); // Kr/min
            double votkm = 1.8; //  kr/km
            if (TU71 == true)
            {
                switch (vdValue)
                {
                    case 3: c1 = 100; c0 = 0.1;
                        break;
                    case 10: c1 = 13; c0 = 10;
                        break;
                    case 11: c1 = 35; c0 = 2;
                        break;
                    case 12: c1 = 35; c0 = 4;
                        break;
                    case 13: c1 = 20; c0 = 6;
                        break;
                    case 14: c1 = 22.22; c0 = 6;
                        break;

                    case 50: c0 = 0; c1 = 4743; c2 = 1.2767; c3 = 1162; c4 = 6; c5 = 2.719; c6 = 0.013043; Qbreak = 1150;
                        break;
                    case 51: c0 = 0.2; c1 = 2083; c2 = 1.4283; c3 = 928; c4 = 4; c5 = 3.835; c6 = 0.015000; Qbreak = 1000;
                        break;
                    case 52: c0 = 0.2; c1 = 1267; c2 = 2; c3 = 802; c4 = 3; c5 = 5.24; c6 = 0.017241; Qbreak = 870;
                        break;
                    case 70: c0 = 0; c1 = 4870; c2 = 0.895; c3 = 1700; c4 = 4; c5 = 3.02; c6 = 0.009677; Qbreak = 2000;
                        break;
                    case 71: c0 = 0; c1 = 4870; c2 = 0.895; c3 = 1400; c4 = 4; c5 = 2.558; c6 = 0.009677; Qbreak = 1550;
                        break;
                    case 90: c0 = 0; c1 = 18808; c2 = 0.7133; c3 = 2091; c4 = 7; c5 = 1.342; c6 = 0.007253; Qbreak = 2000;
                        break;
                    case 99: c0 = 0; c1 = 7500; c2 = 0.5933; c3 = 2091; c4 = 8; c5 = 1.276; c6 = 0.007253; Qbreak = 2000;
                        break;
                    case 89: c0 = 0; c1 = 20000; c2 = 0.625; c3 = 2100; c4 = 10; c5 = 1.109; c6 = (30.0 / 4136.0); Qbreak = 2000;
                        break;
                    default: Console.WriteLine("inget värde hittas");
                        break;
                }

                if (Qbreak != 0)
                {
                    if (X <= Qbreak)
                    {
                        t = ((X / c1) + c2 * (1 + Math.Pow((X / c3), c4))) * length + c0;
                    }
                    else
                    {
                        t = c5 * length + c6 * (X - Qbreak) + c0;
                    }
                }
                else
                {
                    t = (60 * length) / c1 + c0;
                }
                double gc = (t * voth) + (length * votkm);
                return gc;
            }
            else
            {
                // TU06
                switch (vdValue)
                {
                    case 3: f1 = 4655.057; f2 = -764.438; f3 = 111.3553; f4 = -0.227222; f5 = 706E-6; f6 = 1.509553; f7 = 111; // F
                        break;
                    case 10: a1 = 1751.408; a2 = -330.059; a3 = 106.6916; a4 = -0.490124; a5 = 0.00048; a6 = 2.778192; // F
                        break;
                    case 12: f1 = 1719.464; f2 = -312.818; f3 = 89.91307; f4 = -0.340277; f5 = -0.000229; f6 = 2.568559; f7 = 92.0; // F
                        break;
                    case 18: a1 = 1410.327; a2 = -476.534; a3 = 92.48551; a4 = -4.508963; a5 = -0.019723; a6 = 1.048722; // F                     
                        break;
                    case 21: a1 = 1361.633; a2 = -534.663; a3 = 90.42171; a4 = -6.711798; a5 = -0.034075; a6 = 1.57247; // F
                        break;
                    case 25: a1 = 1233.421; a2 = -819.267; a3 = 85.32717; a4 = -18.39336; a5 = -0.098615; a6 = 5.537835;
                        break;
                    case 28: a1 = 1566.197; a2 = -295.33; a3 = 80.61174; a4 = -0.373079; a5 = -0.000883; a6 = 3.055361; // F
                        break;
                    case 30: a1 = 1471.826; a2 = -577.607; a3 = 83.1182; a4 = -6.154758; a5 = -0.041632; a6 = 0.446501; // F
                        break;
                    case 33: a1 = 1420.688; a2 = -486.448; a3 = 76.39183; a4 = -3.862791; a5 = -0.025655; a6 = 0.949777;// F
                        break;
                    case 35: a1 = 1259.373; a2 = -623.67; a3 = 75.18064; a4 = -9.501631; a5 = -0.07382; a6 = 1.802367;// F
                        break;
                    case 37: a1 = 1045.526; a2 = -606.94; a3 = 74.83728; a4 = -12.79281; a5 = -0.076525; a6 = 4.32605;// F
                        break;
                    case 42: f1 = 3314.625; f2 = -613.323; f3 = 63.92766; f4 = -0.254353; f5 = -0.0000166; f6 = 2.529097; f7 = 65.0; f8 = 1118.115; f9 = -282.413; f10 = 58.84709; f11 = -1.008896; f12 = -0.002905; f13 = 2.937746; f14 = 60.0; f15 = 913.12; f16 = 358607.1; f17 = 0.1875; f18 = 12053.18;// F
                        break;
                    case 44: f1 = 3320.104; f2 = -652.956; f3 = 56.26819; f4 = -0.312291; f5 = -0.001356; f6 = 2.475095; f7 = 57.0; f8 = 1117.659; f9 = -317.862; f10 = 55.23467; f11 = -1.506965; f12 = -0.014024; f13 = 1.430703; f14 = 55.0; f15 = 913.12; f16 = 358607.1; f17 = 0.1875; f18 = 12053.18;// F
                        break;
                    case 56: c0 = 0; c1 = 60; // F
                        break;
                    case 60: c0 = 10; c1 = 13; // F
                        break;
                    case 64: c0 = 6; c1 = 22.22; // F
                        break;
                    case 65: c0 = 0; c1 = 4743; c2 = 1.2767; c3 = 1162; c4 = 6; c5 = 2.719; c6 = 3.0 / 230.0; Qbreak = 1150; // F
                        break;
                    case 66: c0 = 0.2; c1 = 2083; c2 = 1.4283; c3 = 928; c4 = 4; c5 = 3.835; c6 = 0.015; Qbreak = 1000; // F
                        break;
                    case 67: c0 = 0.2; c1 = 1267; c2 = 2; c3 = 802; c4 = 3; c5 = 5.24; c6 = 3.0 / 174.0; Qbreak = 870; // F
                        break;
                    case 68: c0 = 0; c1 = 5000; c2 = 0.857; c3 = 1850; c4 = 5; c5 = 1.965; c6 = 3.0 / 310.0; Qbreak = 1800; // F
                        break;
                    case 69: c0 = 0; c1 = 4870; c2 = 0.895; c3 = 1400; c4 = 4; c5 = 2.558; c6 = 3.0 / 310.0; Qbreak = 1550; // F
                        break;
                    case 70: c0 = 0; c1 = 10000; c2 = 0.6522; c3 = 2091; c4 = 7; c5 = 1.061; c6 = 3.0 / 310.0; Qbreak = 1800; //F
                        break;
                    case 71: c0 = 0; c1 = 30000; c2 = 0.5556; c3 = 2200; c4 = 10; c5 = 0.836; c6 = 30.0 / 4136.0; Qbreak = 2000; // F
                        break;
                    case 72: c0 = 0; c1 = 23000; c2 = 0.7595; c3 = 2100; c4 = 10; c5 = 1.313; c6 = 30.0 / 4136.0; Qbreak = 2000; // 
                        break;
                    case 73: c0 = 0; c1 = 20000; c2 = 0.625; c3 = 2100; c4 = 10; c5 = 1.109; c6 = 30.0 / 4136.0; Qbreak = 2000; // F
                        break;
                    case 95: c0 = 2; c1 = 60.0 / 50.0; // F
                        break;
                    default: Console.WriteLine("inget värde hittas");
                        break;
                }
                if (c1 != 0)
                {
                    if (Qbreak != 0)
                    {
                        if (X <= Qbreak)
                        {
                            t = ((X / c1) + c2 * (1 + Math.Pow((X / c3), c4))) * length + c0;
                        }
                        else
                        {
                            t = c5 * length + c6 * (X - Qbreak) + c0;
                        }
                    }
                    else
                    {
                        t = (60 * length) / c1 + c0;
                    }

                }
                if (f1 != 0)
                {
                    if (f7 != 0)
                    {
                        if (kf >= 1.5)
                        {
                            talj = ((Math.Exp(Math.Min(f1, X) / f2) * f3) + f4);
                            namn = ((Math.Exp(Math.Min(f1, X) / f2) + f5) + f6);
                            hel = talj / namn;
                            MINSTA = Math.Min(hel, f7);
                        }
                        else
                        {
                            talj = ((Math.Exp(Math.Min(f8, X) / f9) * f10) + f11);
                            namn = ((Math.Exp(Math.Min(f8, X) / f9) + f12) + f13);
                            hel = talj / namn;
                            MINSTA = Math.Min(hel, f14);
                        }
                        double hejsan = f15 * kf;
                        double hejhej = Math.Min(X, UTILX * hejsan);
                        double hello = (hejhej / (1 - hejhej / hejsan) / f16 + f17 + Math.Max(X / hejsan - UTILX, 0.0) * f18 / hejsan);
                        t = (length * 60) / MINSTA + hello;
                    }
                    else
                    {
                        talj = ((Math.Exp(Math.Min(f1, X) / f2) * f3) + f4);
                        namn = ((Math.Exp(Math.Min(f1, X) / f2) + f5) + f6);
                        hel = talj / namn;
                        t = (60 * length) / Math.Min(hel, f7);
                    }
                }
                if (a1 != 0)
                {
                    talj = ((Math.Exp(Math.Min(a1, X) / a2) * a3) + a4);
                    namn = ((Math.Exp(Math.Min(a1, X) / a2) + a5) + a6);
                    hel = talj / namn;
                    t = (60 * length) / hel;
                }

                double gc = (t * voth) + (length * votkm);
                return gc;
            }


        }
        public double getObjectiveValue(double Qflow)
        {
            double f = 0.0;
            double c1 = 0, c0 = 0, c2 = 0, c5 = 0, c6 = 0, c3 = 0;
            int c4 = 0;
            double Qadd = 0; //oklart vad dena tillför
            Convert.ToDouble(kf);
            double X = (Qflow + Qadd) / kf; //flöde per körfält
            
            switch (vdValue)
            {

                case 3: c1 = 100; c0 = 0.1;
                    break;
                case 10: c1 = 13; c0 = 10;
                    break;
                case 11: c1 = 35; c0 = 2;
                    break;
                case 12: c1 = 35; c0 = 4;
                    break;
                case 13: c1 = 20; c0 = 6;
                    break;
                case 14: c1 = 22.22; c0 = 6;
                    break;
            }
            if (c1 == 0)
            {   // fungerar    

                double intervalBegin = 0;
                double intervalEnd = X;
                double partitionNumbers = 10.0;

                double deltaX = (intervalEnd - intervalBegin) / partitionNumbers;
                double SimpsonsIntegration = (deltaX / 3) * (getTime(intervalBegin, vdValue) + 4 * getTime(intervalBegin + deltaX * 1, vdValue) + 2 * getTime(intervalBegin + deltaX * 2, vdValue) + 4 * getTime(intervalBegin + deltaX * 3, vdValue) + 2 * getTime(intervalBegin + deltaX * 4, vdValue) + 2 * getTime(intervalBegin + deltaX * 5, vdValue) + 4 * getTime(intervalBegin + deltaX * 6, vdValue)
                    + 2 * getTime(intervalBegin + deltaX * 7, vdValue) + 4 * getTime(intervalBegin + deltaX * 8, vdValue) + 2 * getTime(intervalBegin + deltaX * 9, vdValue) + getTime(intervalEnd, vdValue));

                f = SimpsonsIntegration * kf;
            }
            else
            {
                f = SimpsonRule.IntegrateComposite(x => ((60 * length) / c1 + c0), 0, Qflow, 10);
            }
            
            return f * (100.0 / 60.0);
        }


        public string Print()
        {
            string print = id + " " + fromNode + " " + toNode + " " + length + " " + vdValue + " " + ul2 + " " + ul3;
            return print;
        }

        public double getTime(double flow, int vd)
        {
            double c1 = 0, c0 = 0, c2 = 0, c5 = 0, c6 = 0, c3 = 0;
            int c4 = 0; double Qbreak = 0;

            bool TU71 = true; // False if TU06

            double f1 = 0, f2 = 0, f3 = 0, f4 = 0, f5 = 0, f6 = 0, f7 = 0;
            double f8 = 0, f9 = 0, f10 = 0, f11 = 0, f12 = 0, f13 = 0, f14 = 0, f15 = 0, f16 = 0, f17 = 0, f18 = 0;
            double a1 = 0, a2 = 0, a3 = 0, a4 = 0, a5 = 0, a6 = 0;
            double talj = 0, namn = 0, hel = 0, MINSTA = 0;
            double UTILX = 0.95;
            double t = 0;
            Convert.ToDouble(kf);
            double X = flow;
            if (TU71 == true)
            {
                switch (vdValue)
                {
                    case 3: c1 = 100; c0 = 0.1;
                        break;
                    case 10: c1 = 13; c0 = 10;
                        break;
                    case 11: c1 = 35; c0 = 2;
                        break;
                    case 12: c1 = 35; c0 = 4;
                        break;
                    case 13: c1 = 20; c0 = 6;
                        break;
                    case 14: c1 = 22.22; c0 = 6;
                        break;

                    case 50: c0 = 0; c1 = 4743; c2 = 1.2767; c3 = 1162; c4 = 6; c5 = 2.719; c6 = 0.013043; Qbreak = 1150;
                        break;
                    case 51: c0 = 0.2; c1 = 2083; c2 = 1.4283; c3 = 928; c4 = 4; c5 = 3.835; c6 = 0.015000; Qbreak = 1000;
                        break;
                    case 52: c0 = 0.2; c1 = 1267; c2 = 2; c3 = 802; c4 = 3; c5 = 5.24; c6 = 0.017241; Qbreak = 870;
                        break;
                    case 70: c0 = 0; c1 = 4870; c2 = 0.895; c3 = 1700; c4 = 4; c5 = 3.02; c6 = 0.009677; Qbreak = 2000;
                        break;
                    case 71: c0 = 0; c1 = 4870; c2 = 0.895; c3 = 1400; c4 = 4; c5 = 2.558; c6 = 0.009677; Qbreak = 1550;
                        break;
                    case 90: c0 = 0; c1 = 18808; c2 = 0.7133; c3 = 2091; c4 = 7; c5 = 1.342; c6 = 0.007253; Qbreak = 2000;
                        break;
                    case 99: c0 = 0; c1 = 7500; c2 = 0.5933; c3 = 2091; c4 = 8; c5 = 1.276; c6 = 0.007253; Qbreak = 2000;
                        break;
                    case 89: c0 = 0; c1 = 20000; c2 = 0.625; c3 = 2100; c4 = 10; c5 = 1.109; c6 = (30.0 / 4136.0); Qbreak = 2000;
                        break;
                    default: Console.WriteLine("inget värde hittas");
                        break;
                }

                if (Qbreak != 0)
                {
                    if (X <= Qbreak)
                    {
                        t = ((X / c1) + c2 * (1 + Math.Pow((X / c3), c4))) * length + c0;
                    }
                    else
                    {
                        t = c5 * length + c6 * (X - Qbreak) + c0;
                    }
                }
                else
                {
                    t = (60 * length) / c1 + c0;
                }

                return t;
            }
            else
            {
                // TU06
                switch (vdValue)
                {
                    case 3: f1 = 4655.057; f2 = -764.438; f3 = 111.3553; f4 = -0.227222; f5 = 706E-6; f6 = 1.509553; f7 = 111; // F
                        break;
                    case 10: a1 = 1751.408; a2 = -330.059; a3 = 106.6916; a4 = -0.490124; a5 = 0.00048; a6 = 2.778192; // F
                        break;
                    case 12: f1 = 1719.464; f2 = -312.818; f3 = 89.91307; f4 = -0.340277; f5 = -0.000229; f6 = 2.568559; f7 = 92.0; // F
                        break;
                    case 18: a1 = 1410.327; a2 = -476.534; a3 = 92.48551; a4 = -4.508963; a5 = -0.019723; a6 = 1.048722; // F                     
                        break;
                    case 21: a1 = 1361.633; a2 = -534.663; a3 = 90.42171; a4 = -6.711798; a5 = -0.034075; a6 = 1.57247; // F
                        break;
                    case 25: a1 = 1233.421; a2 = -819.267; a3 = 85.32717; a4 = -18.39336; a5 = -0.098615; a6 = 5.537835;
                        break;
                    case 28: a1 = 1566.197; a2 = -295.33; a3 = 80.61174; a4 = -0.373079; a5 = -0.000883; a6 = 3.055361; // F
                        break;
                    case 30: a1 = 1471.826; a2 = -577.607; a3 = 83.1182; a4 = -6.154758; a5 = -0.041632; a6 = 0.446501; // F
                        break;
                    case 33: a1 = 1420.688; a2 = -486.448; a3 = 76.39183; a4 = -3.862791; a5 = -0.025655; a6 = 0.949777;// F
                        break;
                    case 35: a1 = 1259.373; a2 = -623.67; a3 = 75.18064; a4 = -9.501631; a5 = -0.07382; a6 = 1.802367;// F
                        break;
                    case 37: a1 = 1045.526; a2 = -606.94; a3 = 74.83728; a4 = -12.79281; a5 = -0.076525; a6 = 4.32605;// F
                        break;
                    case 42: f1 = 3314.625; f2 = -613.323; f3 = 63.92766; f4 = -0.254353; f5 = -0.0000166; f6 = 2.529097; f7 = 65.0; f8 = 1118.115; f9 = -282.413; f10 = 58.84709; f11 = -1.008896; f12 = -0.002905; f13 = 2.937746; f14 = 60.0; f15 = 913.12; f16 = 358607.1; f17 = 0.1875; f18 = 12053.18;// F
                        break;
                    case 44: f1 = 3320.104; f2 = -652.956; f3 = 56.26819; f4 = -0.312291; f5 = -0.001356; f6 = 2.475095; f7 = 57.0; f8 = 1117.659; f9 = -317.862; f10 = 55.23467; f11 = -1.506965; f12 = -0.014024; f13 = 1.430703; f14 = 55.0; f15 = 913.12; f16 = 358607.1; f17 = 0.1875; f18 = 12053.18;// F
                        break;
                    case 56: c0 = 0; c1 = 60; // F
                        break;
                    case 60: c0 = 10; c1 = 13; // F
                        break;
                    case 64: c0 = 6; c1 = 22.22; // F
                        break;
                    case 65: c0 = 0; c1 = 4743; c2 = 1.2767; c3 = 1162; c4 = 6; c5 = 2.719; c6 = 3.0 / 230.0; Qbreak = 1150; // F
                        break;
                    case 66: c0 = 0.2; c1 = 2083; c2 = 1.4283; c3 = 928; c4 = 4; c5 = 3.835; c6 = 0.015; Qbreak = 1000; // F
                        break;
                    case 67: c0 = 0.2; c1 = 1267; c2 = 2; c3 = 802; c4 = 3; c5 = 5.24; c6 = 3.0 / 174.0; Qbreak = 870; // F
                        break;
                    case 68: c0 = 0; c1 = 5000; c2 = 0.857; c3 = 1850; c4 = 5; c5 = 1.965; c6 = 3.0 / 310.0; Qbreak = 1800; // F
                        break;
                    case 69: c0 = 0; c1 = 4870; c2 = 0.895; c3 = 1400; c4 = 4; c5 = 2.558; c6 = 3.0 / 310.0; Qbreak = 1550; // F
                        break;
                    case 70: c0 = 0; c1 = 10000; c2 = 0.6522; c3 = 2091; c4 = 7; c5 = 1.061; c6 = 3.0 / 310.0; Qbreak = 1800; //F
                        break;
                    case 71: c0 = 0; c1 = 30000; c2 = 0.5556; c3 = 2200; c4 = 10; c5 = 0.836; c6 = 30.0 / 4136.0; Qbreak = 2000; // F
                        break;
                    case 72: c0 = 0; c1 = 23000; c2 = 0.7595; c3 = 2100; c4 = 10; c5 = 1.313; c6 = 30.0 / 4136.0; Qbreak = 2000; // 
                        break;
                    case 73: c0 = 0; c1 = 20000; c2 = 0.625; c3 = 2100; c4 = 10; c5 = 1.109; c6 = 30.0 / 4136.0; Qbreak = 2000; // F
                        break;
                    case 95: c0 = 2; c1 = 60.0 / 50.0; // F
                        break;
                    default: Console.WriteLine("inget värde hittas");
                        break;
                }
                if (c1 != 0)
                {
                    if (Qbreak != 0)
                    {
                        if (X <= Qbreak)
                        {
                            t = ((X / c1) + c2 * (1 + Math.Pow((X / c3), c4))) * length + c0;
                        }
                        else
                        {
                            t = c5 * length + c6 * (X - Qbreak) + c0;
                        }
                    }
                    else
                    {
                        t = (60 * length) / c1 + c0;
                    }

                }
                if (f1 != 0)
                {
                    if (f7 != 0)
                    {
                        if (kf >= 1.5)
                        {
                            talj = ((Math.Exp(Math.Min(f1, X) / f2) * f3) + f4);
                            namn = ((Math.Exp(Math.Min(f1, X) / f2) + f5) + f6);
                            hel = talj / namn;
                            MINSTA = Math.Min(hel, f7);
                        }
                        else
                        {
                            talj = ((Math.Exp(Math.Min(f8, X) / f9) * f10) + f11);
                            namn = ((Math.Exp(Math.Min(f8, X) / f9) + f12) + f13);
                            hel = talj / namn;
                            MINSTA = Math.Min(hel, f14);
                        }
                        double hejsan = f15 * kf;
                        double hejhej = Math.Min(X, UTILX * hejsan);
                        double hello = (hejhej / (1 - hejhej / hejsan) / f16 + f17 + Math.Max(X / hejsan - UTILX, 0.0) * f18 / hejsan);
                        t = (length * 60) / MINSTA + hello;
                    }
                    else
                    {
                        talj = ((Math.Exp(Math.Min(f1, X) / f2) * f3) + f4);
                        namn = ((Math.Exp(Math.Min(f1, X) / f2) + f5) + f6);
                        hel = talj / namn;
                        t = (60 * length) / Math.Min(hel, f7);
                    }
                }
                if (a1 != 0)
                {
                    talj = ((Math.Exp(Math.Min(a1, X) / a2) * a3) + a4);
                    namn = ((Math.Exp(Math.Min(a1, X) / a2) + a5) + a6);
                    hel = talj / namn;
                    t = (60 * length) / hel;
                }
                return t;
            }


        }

    }

}