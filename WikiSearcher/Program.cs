using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace WikiSearcher
{
    internal class Program
    {
        static string wiki_link = "https://en.wikipedia.org";
        static List<string> links  = new List<string>();
        static List<string> link_htmls = new List<string>();
        static List<string> txt_vars = new List<string>();

        static string[] common_words = new string[] { "list", "disambiguation", "wiki", "unit", "time", "group","game", "kirk", "christiansen" };


        static string final_start_link = "https://en.wikipedia.org/wiki/Pancreatic_cancer";
        static string bridge_link = "https://en.wikipedia.org/wiki/Mathematics";
        static string final_target_link = "https://en.wikipedia.org/wiki/Riemann_hypothesis";
        static bool step = false;

        static Random rn = new Random();
        static void Main(string[] args)
        {
            Console.WriteLine("\t\tWiki solver by Unyxe\n\n");
            while (true)
            {
                
                string target_link = "";
                string start_link = "";
                if (!step)
                {
                    links.Clear();
                    link_htmls.Clear();
                    Console.Write("Enter start url: ");
                    //final_start_link = Console.ReadLine();
                    Console.Write("Enter target url: ");
                    //final_target_link = Console.ReadLine();
                    target_link = bridge_link;
                    start_link = final_start_link;
                    links.Add(start_link);
                    link_htmls.Add(GetRawResponse(start_link));
                    txt_vars.Add("start_page");
                } else
                {
                    target_link = final_target_link;
                    start_link = bridge_link;
                }
                Console.WriteLine();
                var tem = target_link.Split('/');
                string target_article_name = tem[tem.Length - 1].ToLower();
                string target_html = GetRawResponse(target_link);
                string[] links_found_on_target = extract_column(GetAllLinks(target_html), 0);
                List<string> key_words_list = new List<string>();
                foreach(string l in links_found_on_target)
                {
                    string link_words = l.Replace("/", "_"); 
                    foreach(string b in link_words.Split('_'))
                    {
                        
                        string j = b.ToLower().Replace("(", String.Empty).Replace(")", String.Empty);
                        j = j.Split('#')[0];
                        if (common_words.Contains(j)) continue;
                        bool is_cont = false;
                        foreach (string gh in key_words_list)
                        {
                            if(gh.Contains(j) || j.Contains(gh))
                            {
                                is_cont = true;
                                break;
                            }
                        }
                        if (is_cont) continue;
                        if (j==target_article_name||j == " "||j==""||key_words_list.Contains(j) ||j.Length<4) continue;
                        

                        key_words_list.Add(j);
                        //Console.WriteLine("RAWKEY:    |" + j + "|");
                    }
                }
                List<string> tempo = SortByFrequency(key_words_list.ToArray()).ToList();
                tempo.Insert(0, target_article_name);
                string[] key_words = tempo.GetRange(0, 5).ToArray();
                foreach (string l in key_words)
                {
                    Console.WriteLine("RAWBKEY:    |" + l + "|");
                }
                //Console.ReadLine();
                while (true)
                {
                    string current_link = links.Last();
                    string link_html = GetRawResponse(current_link);
                    string[] links_found_on_current = extract_column(GetAllLinks(link_html), 0);
                    string[] txt = extract_column(GetAllLinks(link_html), 1);

                    string g = CheckSimil(key_words, links_found_on_current, txt, target_link);
                    Console.Write(g=="sim" ? "Similarity found! " : "Similarity not found... ");
                    //Console.WriteLine(" Current link: " + current_link);
                    Console.WriteLine(" Found link: " + links.Last());
                    if (g == "sim") continue;
                    if (g == "fin")
                    {
                        Console.WriteLine("Pathway FOUND!");
                        break;
                    }

                    int index = 0;
                    links.Add(wiki_link + links_found_on_current[index]);
                    link_htmls.Add(link_html);
                    txt_vars.Add(txt[index]);
                }
                links.Add(target_link);
                link_htmls.Add(target_html);
                txt_vars.Add("target_link");
                if (step)
                {
                    foreach (string g in links)
                    {
                        Console.WriteLine(g);
                    }
                    Console.WriteLine("\n\n");
                }
                step = !step;
            }
        }

        static bool IfContains(string str,string[] s)
        {
            foreach(string st in s)
            {
                if (str.Contains(st))
                {
                    return true;
                }
            }
            return false;
        }
        static string GetRawResponse(string url)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.GetAsync(url).Result;
            byte[] responseData = response.Content.ReadAsByteArrayAsync().Result;
            return Encoding.ASCII.GetString(responseData);
        }
        static string[][] GetAllLinks(string link_html)
        {
            
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(link_html);
            List<string[]> links_found = new List<string[]>();
            int m = 0;
            foreach (HtmlNode link in htmlDoc.DocumentNode.SelectNodes("//a[@href]"))
            {
                string link_str = link.GetAttributeValue("href", "");
                string txt_value = link.InnerText;
                string[] g = new string[] { link_str, txt_value };
                if (link_str == "/wiki/Riemann_hypothesis") Console.ReadLine();
                if (link_str.StartsWith("/wiki/") && !IfContains(link_str, new string[] { ".", ":" }) && !links_found.Contains(g) && link_str != "/wiki/Main_Page" && !links.Contains(wiki_link + link_str))
                {
                    
                    links_found.Add(g);
                    m++;
                }
            }
            return links_found.ToArray();
        }
        static string[] extract_column(string[][] arr, int column_index)
        {
            List<string> c = new List<string>();
            foreach(string[] l in arr)
            {
                c.Add(l[column_index]);
            }
            return c.ToArray();
        }
        
        static string[] GetAllWikiLinks(string link_html)
        {

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(link_html);
            List<string> links_found = new List<string>();
            int m = 0;
            foreach (HtmlNode link in htmlDoc.DocumentNode.SelectNodes("//a[@href]"))
            {
                string link_str = link.GetAttributeValue("href", "");
                if (link_str.StartsWith("/wiki/") && !IfContains(link_str, new string[] { ".", ":" }) && !links_found.Contains(link_str) && link_str != "/wiki/Main_Page" && !links.Contains(wiki_link + link_str))
                {

                    links_found.Add(wiki_link+link_str);
                    m++;
                }
            }
            return links_found.ToArray();
        }
        static string CheckSimil(string[] key_words, string[] searched_links, string[] txt_vars_, string target_link)
        {
            int i = 0;
            foreach (string link in searched_links)
            {

                if (target_link == wiki_link + link) return "fin";

                List<string> key_words_list = new List<string>();
                string link_words = link.Replace("/", "_");
                foreach (string b in link_words.Split('_'))
                {

                    string j = b.ToLower().Replace("(", String.Empty).Replace(")", String.Empty);
                    j = j.Split('#')[0];
                    if (j.Contains("disambiguation") || j.Contains("wiki") || j == " " || j == "" || key_words_list.Contains(j) || j.Length < 4) continue;

                    key_words_list.Add(j);
                }

                foreach (string key_word in key_words)
                {
                    if (key_words_list.Contains(key_word))
                    {
                        //Console.WriteLine("_________" + key_word + ":::::::::" + link);
                        string s_html = GetRawResponse(wiki_link + link);
                        links.Add(wiki_link + link);
                        link_htmls.Add(s_html);
                        txt_vars.Add(txt_vars_[i]);
                        return "sim";
                    }

                }
               
                i++;
            }
            
            return "nosim";
        }
        static string[] SortByFrequency(string[] words)
        {
            List<string> sorted_words = new List<string>();
            int[] occurences = new int[words.Length];
            float sum_occ = 0;
            for(int i =0; i < occurences.Length; i++)
            {
                occurences[i] = 0;
            }
            foreach(string w in words)
            {
                int ind = 0;
                foreach(string w2 in words)
                {
                    if (w.Contains(w2) || w2.Contains(w)) occurences[ind]++;
                    ind++;
                }
            }
            foreach(int i in occurences)
            {
                sum_occ += i;
            }

            for(int i = 0; i < words.Length; i++)
            {
                //Console.WriteLine("OCCUR FIND:     " + words[i] + "   " + occurences[i] / sum_occ * 100 + "%");
            }

            //Sorting
            List<int> sorted_indexes = new List<int>();
            while(sorted_indexes.Count < occurences.Length)
            {
                int max_int = -1;
                int max_index = -1;
                for(int i = 0; i < occurences.Length; i++)
                {
                    if (sorted_indexes.Contains(i)) continue;
                    if(occurences[i] > max_int)
                    {
                        max_int = occurences[i];
                        max_index = i;
                    }
                }
                if(max_index == -1)
                {
                    break;
                }
                sorted_indexes.Add(max_index);
                sorted_words.Add(words[max_index]);
            }

            return sorted_words.ToArray();
        }
    }
}
