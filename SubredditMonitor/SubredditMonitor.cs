////////////////////////////////////////////////////////////////////////////////////////////////////
// file:	SubredditMonitor.cs
//
// summary:	This class will scan a chosen subreddit's latest batch of posts for a chosen keyword. If
// that keyword is found it will send an email via GMail to the provided address (SMS can be sent
// via GMail if the correct domain is used). 
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Threading;

namespace SubredditMonitor
{
    class SubredditMonitor
    {
        //SET THESE VARS TO SUCCESSFULLY SEND AN EMAIL
        public string gmailRecipient = ""; //ex: 8887776666@vtext.com or 8887776666@txt.att.net for sms
        public string gmailUsername = ""; //ex: blabla@gmail.com
        public string gmailPw = ""; //ex: hunter1
        public string gmailSubject = ""; //ex: Subreddit Monitor

        static void Main(string[] args)
        {
            SubredditMonitor monitor = new SubredditMonitor();

            string subredditName = "XboxOne"; //the subreddit to search
            string keyWord = "deal".ToLower(); //the keyword to scan for

            int sleepTime = 30; //seconds to wait
            HttpClient client = new HttpClient();
            List<string> records = new List<string>(); //a list to make sure we don't resend a record if we already have
            var emailBody = string.Empty;

            var uri = new Uri(String.Format("https://www.reddit.com/r/{0}/new/.json", subredditName));
            try
            {
                while (true)
                {
                    HttpResponseMessage response = client.GetAsync(uri).Result;
                    var results = response.Content.ReadAsStringAsync().Result;

                    try
                    {
                        var jsonResults = JsonConvert.DeserializeObject<ApiReturn>(results);

                        Console.WriteLine("********");
                        Console.WriteLine(DateTime.Now + ": New fetch cycle");

                        foreach (ApiReturnDataChildren child in jsonResults.data.children)
                        {
                            Console.WriteLine("Checking: " + child.data.permalink);
                            if (child.data.title.ToLower().Contains(keyWord) && !records.Contains(child.data.permalink))
                            {
                                Console.WriteLine("Match Found: " + child.data.permalink);

                                records.Add(child.data.permalink);
                                emailBody += "********" + "<br />";
                                emailBody += child.data.title + "<br />";
                                emailBody += "http://www.reddit.com/" + child.data.permalink + "<br />";
                                emailBody += child.data.url + "<br />";
                            }
                        }

                        if (emailBody != string.Empty)
                        {
                            monitor.SendEmailWithBody(emailBody);
                            emailBody = string.Empty;
                        }

                        Thread.Sleep(1000 * sleepTime);
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(60000); //sleep for a minute and continue if the server seems down
                        monitor.SendEmailWithBody("Exception, will continue: " + ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                monitor. SendEmailWithBody("Exception, execution stopped: " + ex.ToString());
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Sends an email with body provided using GMail.  
        ///             IMPORTANT: The GMail account must be set to allow less secure apps to access the 
        ///             account. See:
        ///             https://support.google.com/accounts/answer/6010255?hl=en
        ///             </summary>
        ///
        /// <remarks>   John, 10/10/2016. </remarks>
        ///
        /// <param name="body"> The body. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void SendEmailWithBody(string body)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.IsBodyHtml = true;

                mail.From = new MailAddress(gmailUsername);
                mail.To.Add(gmailRecipient);
                mail.Subject = gmailSubject;
                mail.Body = body;

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential(gmailUsername, gmailPw);
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);

                mail = null;
                SmtpServer = null;
            }
            catch (Exception ex)
            {
            }
        }

    }

    //MODELS
    public class ApiReturn
    {
        public string kind { get; set; }
        public ApiReturnData data { get; set; }
    }

    public class ApiReturnData
    {
        public string modhash { get; set; }
        public List<ApiReturnDataChildren> children { get; set; }
        public string after { get; set; }
        public string before { get; set; }
    }

    public class ApiReturnDataChildren
    {
        public string kind { get; set; }
        public ApiReturnDataChildrenData data { get; set; }
    }

    public class ApiReturnDataChildrenData
    {
        public bool contest_mode { get; set; }
        public string banned_by{ get; set; }
        public string domain { get; set; }
        public string subreddit { get; set; }
        public string selftext_html { get; set; }
        public string selftext { get; set; }
        public string likes { get; set; }
        public string suggested_sort { get; set; }
        public List<string> user_reports { get; set; }
        public string saved { get; set; }
        public string id { get; set; }
        public string gilded { get; set; }
        public string clicked { get; set; }
        public string report_reasons { get; set; }
        public string author { get; set; }
        public string name { get; set; }
        public string score { get; set; }
        public string approved_by { get; set; }
        public string over_18 { get; set; }
        public string removal_reason { get; set; }
        public string hidden { get; set; }
        public string thumbnail { get; set; }
        public string subreddit_id { get; set; }
        public string edited { get; set; }
        public string link_flair_css_class { get; set; }
        public string author_flair_css_class { get; set; }
        public string downs { get; set; }
        public List<string> mod_reports { get; set; }
        public string archived { get; set; }
        public string is_self { get; set; }
        public string hide_score { get; set; }
        public string permalink { get; set; }
        public string locked { get; set; }
        public string stickied { get; set; }
        public string created { get; set; }
        public string url { get; set; }
        public string author_flair_text { get; set; }
        public string quarantine { get; set; }
        public string title { get; set; }
        public string created_utc { get; set; }
        public string link_flair_text { get; set; }
        public string distinguished { get; set; }
        public string num_comments { get; set; }
        public string visited { get; set; }
        public string num_reports { get; set; }
        public string ups { get; set; }
    }
}
