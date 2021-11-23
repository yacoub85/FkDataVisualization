using System;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FkDataVisualization
{
    class Program
    {
        //create a static instance of HttpClient to handle requests and responses.
        private static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            Console.Title = "Forsakringskassans data visualization";

            string menDataUrl = "https://www.forsakringskassan.se/fk_apps/MEKAREST/public/v1/iv-planerad/IVplaneradvardland.json?kon_kod=M";
            string womenDataUrl = "https://www.forsakringskassan.se/fk_apps/MEKAREST/public/v1/iv-planerad/IVplaneradvardland.json?kon_kod=K";

            string countryCodeForMen = "";
            string countryCodeForWomen = "";
            uint numOfMen = 0;
            uint numOfWomen = 0;

            //make a Get request and retrieve the response from Forsakringskassans API.
            var GetDataForMenStr = client.GetStringAsync(menDataUrl);
            var GetDataForWomenStr = client.GetStringAsync(womenDataUrl);
            var resMen="";
            var resWomen = "";
            try
            {
                 resMen = await GetDataForMenStr;
                resWomen = await GetDataForWomenStr;
            }
            catch (Exception)
            {
                Console.WriteLine("Errore!");           
            }
            

            //Represents a JSON array.
            var dataObjMen = JArray.Parse(resMen);
            var dataObjWomen = JArray.Parse(resWomen);

            //Extract the required data from the API and save it in a list
            List<FkAPIData> fkAPIData = new List<FkAPIData>();
            for (int i = 0; i < dataObjMen.Count; i++)
            {
                // Extract the women's data 
                countryCodeForWomen = dataObjWomen[i]["dimensions"]["vardland_kod"].ToString();
                try
                {
                    numOfWomen = uint.Parse(dataObjWomen[i]["observations"]["antal"]["value"].ToString());
                }
                catch (Exception)
                {
                    numOfWomen = 0;
                }

                // Extract the men's data 
                countryCodeForMen = dataObjMen[i]["dimensions"]["vardland_kod"].ToString();
                try
                {
                    numOfMen = uint.Parse(dataObjMen[i]["observations"]["antal"]["value"].ToString());
                }
                catch (Exception)
                {
                    numOfMen = 0;
                }

                // Save only data that has values over zero for visualize it
                if ((countryCodeForMen != "ALL") && numOfMen + numOfWomen > 0 && countryCodeForMen == countryCodeForWomen)
                {
                    fkAPIData.Add(new FkAPIData(countryCodeForMen, numOfMen, numOfWomen));
                }
                if (countryCodeForMen != countryCodeForWomen)
                {
                    Console.WriteLine("There may have been an error fetching the data from the API!");
                }

            }

            // Create sorted list
            List<FkAPIData> sortFkAPIData = new List<FkAPIData>();
            uint mini = 0; uint max = 0;
            int index = 0; int length = fkAPIData.Count;

            for (int i = 0; i < length; i++)
            {
                foreach (var item in fkAPIData)
                {
                    mini = item.Get_number_of_women() + item.Get_number_of_men();
                    if (mini >= max)
                    {
                        max = mini;
                        index = fkAPIData.IndexOf(item);
                    }

                }
                sortFkAPIData.Add(fkAPIData[index]);
                fkAPIData.RemoveAt(index);
                index = 0;
                max = 0;
            }


            //Prepare the data for visualize it
            uint maxNumInSample = sortFkAPIData[0].Get_number_of_men();
            if (maxNumInSample < sortFkAPIData[0].Get_number_of_women())
            {
                maxNumInSample = sortFkAPIData[0].Get_number_of_women();
            }

            //Call drawing graph function
            draw_graph(maxNumInSample, sortFkAPIData.Count,sortFkAPIData);
            Console.ReadLine();

        }

        // Graph function
        public static void draw_graph(uint x_axel_max_value, int y_axel_length, List<FkAPIData> values)
        {
            
            Console.WriteLine("Number of recipients");
            //Rounding Column X to the Hundreds
            if (x_axel_max_value % 100 != 0)
            {
                x_axel_max_value = (100 - x_axel_max_value % 100) + x_axel_max_value;
            }
            //Divide column scale by tens
            x_axel_max_value = x_axel_max_value / 10;

            //Handling maximum Input for drawing the graph 
            if (x_axel_max_value >= 1000)
            {
                x_axel_max_value = 999;
            }
            if (y_axel_length > 20)
            {
                y_axel_length = 20;
            }

            // Draw the graph
            for (uint i = x_axel_max_value; i > 0; i--)
            {
                if ((i / 10) < 1)
                {
                    Console.Write(" ");
                }
                Console.Write(i * 10 + "|");

                for (int j = 0; j < y_axel_length; j++)
                {

                    if (values[j].Get_number_of_men() >= i * 10)
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write("  ");

                        Console.BackgroundColor = ConsoleColor.Blue;
                        Console.Write(" ");
                        Console.BackgroundColor = ConsoleColor.Black;
                    }

                    if (values[j].Get_number_of_men() < i * 10)
                    {
                        Console.Write("   ");
                    }

                    if (values[j].Get_number_of_women() >= i * 10)
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.Write(" ");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.Write(" ");
                    }
                    if (values[j].Get_number_of_women() < i * 10)
                    {
                        Console.Write("  ");
                    }

                }
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;

            }

            Console.Write("  0|");

            for (uint i = 0; i < y_axel_length * 5; i++)
            {
                Console.Write("_");
            }
            Console.WriteLine("");
            Console.Write("     ");
            for (int i = 0; i < y_axel_length; i++)
            {
                //Handling incorrect entry if country code is less than or greater than two characters
                if (values[i].Get_country_code().Length >= 2)
                {
                    Console.Write(" " + values[i].Get_country_code().Substring(0, 2) + "  ");
                }
                else if (values[i].Get_country_code().Length == 1)
                {
                    Console.Write(" " + values[i].Get_country_code() + " " + "  ");
                }
                else
                    Console.Write(" " + "  " + "  ");
            }
            Console.Write("Country");
            //Print out colors indication
            Console.Write("\n\n\t");
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.Write(" ");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(" Men\t");

            Console.BackgroundColor = ConsoleColor.Red;
            Console.Write(" ");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(" Women");


        }

    }
}
