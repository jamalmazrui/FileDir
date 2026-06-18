// Encoding.GetEncoding(response.CharacterSet));
// int index = contentDisposition.IndexOf(lookFor, StringComparison.CurrentCultureIgnoreCase);

using System;
using System.IO;
using System.Net;
using System.Net.Mime;

public class Program {
static void Main (string[] args) {
if (args.Length == 0) throw new ApplicationException ("Specify the URL of the file to download.");

string sUrl = args[0];
WebClient client = new WebClient ();

client.Headers.Add ("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

byte[] bData = client.DownloadData(sUrl);
string sName = "file.tmp";
string sDir = @"c:\temp";
string sDefaultFullName = Path.Combine(sDir, sName);

string sContentDisposition = client.ResponseHeaders["Content-Disposition"];
if (!string.IsNullOrEmpty(contentDisposition)) sName = new ContentDisposition(sContentDisposition).FileName;
else {
sLocation = client.ResponseHeaders("Location"];
if (!StringIsNullOrEmpty(sLocation) sName = Path.GetFileName(sLocation);
}
sPath = new Uri(sUrl).LocalPath;
sName = Path.GetFileName(sPath);
