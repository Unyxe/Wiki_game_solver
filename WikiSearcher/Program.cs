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


        static string start_link = "";
        static string target_link = "";

        static Random rn = new Random();
        static void Main(string[] args)
        {
            Console.WriteLine("\t\tWiki solver by Unyxe\n\n");
            while (true)
            {
                links.Clear();
                link_htmls.Clear();
                Console.Write("Enter start url: ");
                start_link = Console.ReadLine();
                Console.Write("Enter target url: ");
                target_link = Console.ReadLine();
                links.Add(start_link);
                link_htmls.Add(GetRawResponse(start_link));
                string target_html = GetRawResponse(target_link);
                string[] links_found_on_target = GetAllLinks(target_html);
                while (true)
                {
                    string current_link = links.Last();
                    string link_html = GetRawResponse(current_link);
                    string[] links_found_on_current = GetAllLinks(link_html);

                    bool g = CheckSimil(links_found_on_target, links_found_on_current);
                    Console.WriteLine(g ? "Found!" : "Continue..." + " Current link: " + current_link);

                    if (g)
                    {
                        break;
                    }

                    int index = 0;
                    links.Add(wiki_link + links_found_on_current[index]);
                    link_htmls.Add(link_html);
                }
                links.Add(target_link);
                link_htmls.Add(target_html);
                Console.WriteLine();
                Console.WriteLine("Optimisation...");
                Console.WriteLine();
                Optimise();
                foreach (string g in links)
                {
                    Console.WriteLine(g);
                }
                Console.WriteLine("\n\n");
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
        static string[] GetAllLinks(string link_html)
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
                    
                    links_found.Add(link_str);
                    m++;
                }
            }
            return links_found.ToArray();
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
        static void Optimise()
        {
            int l = links.Count;
            for(int i = 0; i <l; i++)
            {
                string[] links_found = GetAllWikiLinks(link_htmls[i]);
                for (int j = i+2;j<l;j++)
                {
                    if (links_found.Contains(links[j]))
                    {
                        for(int k = i+1;k < j; k++)
                        {
                            int gh = links.IndexOf(links[k]);
                            links.RemoveAt(gh);
                            link_htmls.RemoveAt(gh);
                        }
                    }
                }
            }
            
        }
        static void PrintAllLinks(string  link_url)
        {
            Console.WriteLine("_______________________________");
            foreach(string s in GetAllLinks(link_url))
            {
                Console.WriteLine(s);
            }
            Console.WriteLine("_______________________________");
        }
        static bool CheckSimil(string[] a1, string[] a2)
        {
            foreach(string s in a1)
            {
                if (a2.Contains(s))
                {
                    string s_html = GetRawResponse(wiki_link + s);
                    string[] b = GetAllLinks(s_html);
                    foreach(string g in b)
                    {
                        if(wiki_link+g == target_link)
                        {
                            if (wiki_link + s != target_link)
                            {
                                links.Add(wiki_link + s);
                                link_htmls.Add(s_html);
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
