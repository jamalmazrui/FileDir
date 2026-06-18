using System;
using System.Net;
using System.IO;

public class Test
{
public static void Main (string[] args)
{
if (args.Length == 0) {
throw new ApplicationException ("Specify the URI of the resource to retrieve.");
}

string sUrl = args[0];
WebClient client = new WebClient ();

// Add a user agent header in case the
// requested URI contains a query.
client.Headers.Add ("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

byte[] bData = client.DownloadData(sUrl);
string sFile = @"C:\temp\temp.tmp";
File.WriteAllBytes(sFile, bData);
}
}
