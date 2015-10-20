/*
* FILE : Program.cs
* PROJECT : PROG3080 - Assignment #1
* PROGRAMMER : Constantine Grigoriadis, Sunny Mangat, Dylan Sawchuk, Nick Whitey
* FIRST VERSION : 2014-11-28
* DESCRIPTION : Totals a purchase given a region and subtotal
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HL7parser;

namespace GIROP_Purchase_Totaller
{
    /// <summary>
    /// Generates a purchase reciept
    /// </summary>
    class PurchaseReciept
    {
        //variables used to hold values
        private string regionCode;
        private double HSTRate;
        private double PSTRate;
        private double GSTRate;
        private double HST_Total;
        private double PST_Total;
        private double GST_Total;
        private double subTotal;
        private double grandTotal;

        //Getters
        //Get the region code for the province
        public string RegionCode { get { return regionCode; } }

        //Get the HST Rate
        public double HST_Rate { get { return HSTRate; } }

        //Get the PST Rate
        public double PST_Rate { get { return PSTRate; } }

        //Get the GST Rate
        public double GST_Rate { get { return GSTRate; } }

        //Get the calculated HST
        public double cal_HST { get { return HST_Total; } }

        //Get the calculated PST
        public double cal_PST { get { return PST_Total; } }

        //Get the calculated GST 
        public double cal_GST { get { return GST_Total; } }

        //Get the subtotal
        public double SubTotal { get { return subTotal; } }

        //Get the grandTotal
        public double GrandTotal { get { return grandTotal; } }

        //constructor
        public PurchaseReciept (string Region_Code, double purchaseSubTotal)
        {
            regionCode = Region_Code.ToUpper ();
            HSTRate = 0.0;
            PSTRate = 0.0;
            GSTRate = 0.0;
            subTotal = purchaseSubTotal;
            grandTotal = 0.0;
        }


        /// <summary>
        /// Calculates Purchase 
        /// </summary>
        public void CalculateTotalPurchase ()
        {
            switch (regionCode)
            {
                case "NL":
                    HSTRate = 0.13;
                    break;
                case "NS":
                    HSTRate = 0.15;
                    break;
                case "NB":
                    HSTRate = 0.13;
                    break;
                case "PE":
                    PSTRate = 0.10;
                    GSTRate = 0.05;
                    break;
                case "QC":
                    PSTRate = 0.095;
                    GSTRate = 0.05;
                    break;
                case "ON":
                    HSTRate = 0.13;
                    break;
                case "MB":
                    PSTRate = 0.07;
                    GSTRate = 0.05;
                    break;
                case "SK":
                    PSTRate = 0.05;
                    GSTRate = 0.05;
                    break;
                case "AB":
                    GSTRate = 0.05;
                    break;
                case "BC":
                    HSTRate = 0.12;
                    break;
                case "YT":
                    GSTRate = 0.05;
                    break;
                case "NT":
                    GSTRate = 0.05;
                    break;
                case "NU":
                    GSTRate = 0.05;
                    break;
                default:
                    throw new ThortonsSOAException ("region unknown", -3);
            }//end switch

            if (subTotal < 0)//TODO
            {
                throw new ThortonsSOAException ("negative subtotal", -3);
            }

            if (HSTRate > 0.0)
            {
                HST_Total = subTotal * HSTRate;
                grandTotal = HST_Total + subTotal;
            }
            else if (regionCode == "PE" || regionCode == "QC")
            {
                GST_Total = subTotal * GSTRate;
                PST_Total = PSTRate * (subTotal + GST_Total);
                grandTotal = subTotal + GST_Total + PST_Total;
            }
            else
            {
                GST_Total = subTotal * GSTRate;
                PST_Total = subTotal * PSTRate;
                grandTotal = subTotal + GST_Total + PST_Total;
            }

            grandTotal = Math.Round (grandTotal, 2);
        }//public void CalculateTotalPurchase()

    }//class PurchaseReciept
}//namespace GIROP_Purchase_Totaller
