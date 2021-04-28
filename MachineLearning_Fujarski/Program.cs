using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Collections;

/**
 * Created by Łukasz Fujarski
 * 2021
 * 
 * Przedmiot: System uczące się
 * Prowadzący: prof. dr hab. Jan Kozak
 * */

namespace MachineLearning_Fujarski
{
    class Program
    {
        private static Dictionary<int, Dictionary<string, double>> slownikWystapienWartosci = new Dictionary<int, Dictionary<string, double>>();
        private static List<double> listOfInformationFunction = new List<double>();
        private static List<double> listOfInformationGain = new List<double>();
        private static List<double> listOfEntropiesOfAttributes = new List<double>();
        private static List<double> listOfGainRatios = new List<double>();
        private static List<List<string>> listaLiczbWartosci = new List<List<string>>();
        private static List<Dictionary<string, Dictionary<string, int>>> listOfDictionariesOfAttributesDecision = new List<Dictionary<string, Dictionary<string, int>>>();
        private static double entropyOfDecisionColumn;
        private static List<List<string>> mainList = new List<List<string>>();
        private static string[] lines;

        static void Main(string[] args)
        {
            loadFile();
            object tree = buildATree(mainList);
            Console.WriteLine("Wizualizacja:");
            DrawVisualization(tree);
        }

        static void DrawVisualization(object tree, string tabs = "")
        {
            Type t = tree.GetType();
            bool isDict = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>);
            if (isDict)
            {
                IDictionary dict = tree as IDictionary;
                int i = 0;
                foreach (Object obj in dict.Values)
                {
                    string foundEl = String.Empty;

                    int j = 0;
                    foreach (Object obj2 in dict.Keys)
                    {
                        foundEl = (string)obj2;
                        if (j == i) break;
                        j++;
                    }

                    Console.Write("\n" + tabs + foundEl + " -> ");
                    DrawVisualization(obj, tabs + "    ");
                    i++;
                }
            }
            else
            {
                Console.Write("D: " + tree);
            }
        }

       static object buildATree(List<List<string>> data, object decision = null)
        {
            if (!stopCondition())
            {
                object result = null;
                (
                    Dictionary<string, List<List<string>>> dictOfChildrenData, 
                    Dictionary<string, Dictionary<string, object>> dictVisualization, 
                    string atrIndex
                ) = chooseBestDivision(data);

                foreach (KeyValuePair<string, List<List<string>>> children in dictOfChildrenData)
                {                    
                    result = buildATree(children.Value, dictVisualization[atrIndex][children.Key]);

                    if (result.GetType() == typeof(bool) && (bool)result == false)
                    {
                        return decision;
                    }
                    else
                    {
                        dictVisualization[atrIndex][children.Key] = result;
                    }
                }

                return dictVisualization;
            }
            else
            {
                listOfGainRatios.Clear();
                return false;
            }
        }

        static bool stopCondition()
        {
            if (listOfGainRatios.Count() > 0)
            {
                double maks = listOfGainRatios.Max();
                if (maks > 0)
                    return false;
                else return true;
            }
            else return false;
        }

        static void loadFile()
        {
            lines = System.IO.File.ReadAllLines(@"C:\Users\overc0de\Desktop\MachineLearning\MachineLearning_Fujarski\MachineLearning_Fujarski\test_data\breast-cancer.data");
            for (int i = 0; i < lines.Length; i++)
            {
                string[] res = lines[i].Split(',');
                List<string> tempList = new List<string>();
                for (int j = 0; j < res.Length; j++)
                {
                    tempList.Add(res[j]);
                }
                mainList.Add(tempList);
            }
        }

        static (Dictionary<string, List<List<string>>>, Dictionary<string, Dictionary<string, object>>, string) chooseBestDivision(List<List<string>> tablicaPotomka)
        {
            slownikWystapienWartosci.Clear();
            listOfInformationFunction.Clear();
            listOfInformationGain.Clear();
            listOfGainRatios.Clear();
            listOfEntropiesOfAttributes.Clear();
            listOfDictionariesOfAttributesDecision.Clear();

            int i = 0;
            foreach(List<string> lista in tablicaPotomka)
            {
                int j = 0;
                foreach(string el in lista)
                {
                    if (i == 0)
                    {
                        slownikWystapienWartosci[j] = new Dictionary<string, double>();
                        List<string> list = new List<string>();
                        listaLiczbWartosci.Add(list);
                    }
                    j++;
                }
                i++;
            }

            for(int j = 0; j < tablicaPotomka[0].Count(); j++)
            {
                listOfDictionariesOfAttributesDecision.Add(new Dictionary<string, Dictionary<string, int>>());
            }

            foreach(List<string> lista in tablicaPotomka)
            {
                int j = 0;
                foreach(string el in lista)
                {   
                    if (!slownikWystapienWartosci[j].ContainsKey(el))
                    {
                        slownikWystapienWartosci[j][el] = 0;
                    }
                    slownikWystapienWartosci[j][el]++;

                    if (!listaLiczbWartosci[j].Contains(el))
                    {
                        listaLiczbWartosci[j].Add(el);
                    }
                    j++;
                }
            }

            i = 0;
            foreach(List<string> lista in tablicaPotomka)
            {
                int j = 0;
                foreach(string el in lista)
                {
                    if (!listOfDictionariesOfAttributesDecision[j].ContainsKey(el))
                        listOfDictionariesOfAttributesDecision[j][el] = new Dictionary<string, int>();
                    if (!listOfDictionariesOfAttributesDecision[j][el].ContainsKey(tablicaPotomka[i][lista.Count() - 1]))
                        listOfDictionariesOfAttributesDecision[j][el][tablicaPotomka[i][lista.Count() - 1]] = 1;
                    else
                        listOfDictionariesOfAttributesDecision[j][el][tablicaPotomka[i][lista.Count() - 1]] += 1;
                    j++;
                }
                i++;
            }

            /* Liczba wartości */
            //ShowNumberOfValues();

            /* Liczba wystąpień */
            //ShowNumberOfOccurrences();

            Dictionary<string, double> decisionColumn = slownikWystapienWartosci[slownikWystapienWartosci.Count - 1];
            Dictionary<string, double> decisionColumnProbabilities = new Dictionary<string, double>();
            foreach (KeyValuePair<string, double> kvp in decisionColumn)
            {
                decisionColumnProbabilities[kvp.Key] = (double)kvp.Value / (double)tablicaPotomka.Count();
            }
            entropyOfDecisionColumn = CalculateEntropy(decisionColumnProbabilities);

            // Obliczanie funkcji informacji
            CalculateInformationFunctionValue(tablicaPotomka);

            // Obliczanie przyrostu informacji
            CalculateInformationGain();

            // Wyznaczanie listy entropii atrybutow dla zrown. przyrostu
            CalculateListOfEntropiesOfAttributes(tablicaPotomka);

            // Wyznaczanie zwrownaż. przyrostu informacji
            int highestGainRatioIndex = CalculateGainRatios(tablicaPotomka);

            Dictionary<string, object> dictVisualization = new Dictionary<string, object>();
            Dictionary<string, List<List<string>>> dictOfChildrenData = new Dictionary<string, List<List<string>>>();
            foreach (KeyValuePair<string, double> kvp in slownikWystapienWartosci[highestGainRatioIndex])
            {
                List<List<string>> danePotomka = new List<List<string>>();

                foreach (List<string> wiersz in tablicaPotomka)
                {
                    if (wiersz[highestGainRatioIndex] == kvp.Key)
                    {
                        danePotomka.Add(wiersz);
                    }
                }
                dictOfChildrenData[kvp.Key] = danePotomka;
                dictVisualization[kvp.Key] = danePotomka[0][danePotomka[0].Count() - 1];
            }

            string atrIndex = String.Format("a{0}", highestGainRatioIndex + 1);
            Dictionary<string, Dictionary<string, object>> dictVisualization2 = new Dictionary<string, Dictionary<string, object>>();
            dictVisualization2[atrIndex] = dictVisualization;
            
            return (dictOfChildrenData, dictVisualization2, atrIndex);
        }

        static int CalculateGainRatios(List<List<string>> tablicaPotomka)
        {
            for (int i = 0; i < tablicaPotomka[0].Count() - 1; i++)
            {
                double splitInfo = listOfEntropiesOfAttributes[i];
                double gainRatio = 0;
                if (splitInfo > 0)
                    gainRatio = listOfInformationGain[i] / splitInfo;
                else gainRatio = 0;

                listOfGainRatios.Add(gainRatio);
            }

            return listOfGainRatios.IndexOf(listOfGainRatios.Max());
        }

        static void CalculateListOfEntropiesOfAttributes(List<List<string>> tablicaPotomka)
        {
            for (int i = 0; i < tablicaPotomka[0].Count() - 1; i++)
            {
                Dictionary<string, double> probabilitiesOfAttributes = new Dictionary<string, double>();

                foreach (KeyValuePair<string, double> kvp in slownikWystapienWartosci[i])
                {
                    double probabilityOfAttrValue = ((double)kvp.Value / (double)tablicaPotomka.Count());

                    probabilitiesOfAttributes[kvp.Key] = probabilityOfAttrValue;
                }

                double entropyOfAttribute = CalculateEntropy(probabilitiesOfAttributes);
                listOfEntropiesOfAttributes.Add(entropyOfAttribute);
            }
        }

        static void CalculateInformationGain()
        {
            foreach (double info in listOfInformationFunction)
            {
                double gain = entropyOfDecisionColumn - info;
                listOfInformationGain.Add(gain);
            }
        }

        static void CalculateInformationFunctionValue(List<List<string>> tablicaPotomka)
        {
            for (int i = 0; i < tablicaPotomka[0].Count() - 1; i++)
            {
                double informationFunction = 0;

                foreach (KeyValuePair<string, double> kvp in slownikWystapienWartosci[i])
                {
                    Dictionary<string, double> decisionsDict = new Dictionary<string, double>();

                    double probabilityOfAttrValue = ((double)kvp.Value / (double)tablicaPotomka.Count());

                    foreach (KeyValuePair<string, int> kvp2 in listOfDictionariesOfAttributesDecision[i][kvp.Key])
                    {
                        decisionsDict[kvp2.Key] = (double)kvp2.Value / (double)kvp.Value;
                    }

                    if (decisionsDict.Count < 2)
                    {
                        decisionsDict["null"] = 0;
                    }
                    double entropyOfValue = CalculateEntropy(decisionsDict);
                    informationFunction += (probabilityOfAttrValue * entropyOfValue);
                }
                listOfInformationFunction.Add(informationFunction);
            }
        }

        static void ShowNumberOfValues()
        {
            Console.WriteLine();
            Console.WriteLine("Liczba wartości: ");
            int counter = 0;
            foreach (List<string> list in listaLiczbWartosci)
            {
                Console.WriteLine("Zmienna " + counter + ":");
                foreach (string el in list)
                {
                    Console.Write(el + " ");
                }
                Console.WriteLine();
                counter++;
            }
        }

        static void ShowNumberOfOccurrences()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Ilość wystąpień: ");
            foreach (KeyValuePair<int, Dictionary<string, double>> kvp in slownikWystapienWartosci)
            {
                Console.WriteLine("Atrybut " + kvp.Key + ": ");
                foreach (KeyValuePair<string, double> kvp2 in kvp.Value)
                {
                    Console.WriteLine(kvp2.Key + " => " + kvp2.Value);
                }
            }
        }

        static double CalculateEntropy(Dictionary<string, double> dict)
        {
            double entropy = 0;
            foreach (KeyValuePair<string, double> kvp2 in dict)
            {
                double probability = (double)kvp2.Value;
                if (probability > 0)
                {
                    double log = (double)Math.Log(probability, 2);
                    double entropyEl = (probability * log);
                    entropy += entropyEl;
                }
            }
            if (entropy != 0)
                entropy = -1 * entropy;
            return entropy;
        }
    }
}
