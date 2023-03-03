//This code requires FFMC package
//Made for .NET 7.0

using FFMC;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

bool ServerWorking = false;
Console.Write("Server address: ");
string address = Console.ReadLine();
async void StartServer()
{
    Socket listener = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
    Console.Write("This PC address: ");
    listener.Bind(new IPEndPoint(IPAddress.Parse(Console.ReadLine()), 80));
    listener.Listen();
    if (!ServerWorking)
    {
        ServerWorking = true;
        Console.WriteLine("Server started up!");
        Task.Run(() =>
        {

            while (ServerWorking)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(AddClientThread), listener.Accept());
            }
        });
        while (true)
        {
            if (Console.ReadKey().KeyChar == 'c')
                Console.Clear();
        }
    }
}
void AddClientThread(object socket)
{
    try
    {
        Socket s = socket as Socket;
        byte[] buffer = new byte[s.ReceiveBufferSize];
        int l = s.Receive(buffer);
        string[] headers = GetHttpHeaders(Encoding.UTF8.GetString(buffer));
        Console.WriteLine(Encoding.UTF8.GetString(buffer));
        Console.WriteLine($"{headers.Length} = 3: {3.Equals(headers.Length)}");
        if (headers.Length == 3)
        {
            StreamWriter sw = new StreamWriter($"{Environment.CurrentDirectory}{headers[0]}\\{headers[2]}");
            string a = Encoding.UTF8.GetString(buffer);
            string after = a.Split("Content-Disposition: ")[1].Split('\n')[3];
            int index = a.IndexOf(after);
            Console.WriteLine(l - (index + 50));
            sw.BaseStream.Write(buffer,index,l-(index+46));
            sw.Flush();
            sw.Close();
        }
        if (headers[0].LastIndexOf('.') == -1)
            headers[0] += ".html";
        if (!System.IO.File.Exists(Environment.CurrentDirectory + headers[0]))
            headers[0] = "\\main.html";
        FileStream fs = new FileStream(Environment.CurrentDirectory + headers[0], FileMode.Open, FileAccess.Read);
        string head;
        if (fs.Name.EndsWith("FPASST.txt"))
            return;
        if (fs.Name.EndsWith("html") || fs.Name.EndsWith("htm"))
            head = $"HTTP/1.1 200 OK\nContent-type: text/html\nContent-Length: {fs.Length}\n\n";
        else
            head = $"HTTP/1.1 200 OK\nContent-type: application/unknown\nContent-Length: {fs.Length}\n\n";
        Console.WriteLine(fs.Name);
        if((fs.Name.EndsWith("fileman.scpp")))
            a(s, headers[1]);
        else
        {
            byte[] data_headers = Encoding.UTF8.GetBytes(head);
            s.Send(data_headers, data_headers.Length, SocketFlags.None);
            while (fs.Position < fs.Length)
            {
                byte[] data = new byte[1024];
                int length = fs.Read(data, 0, data.Length);
                s.Send(data, length, SocketFlags.Partial);
            }
            s.Close();
        }
        fs.Close();
    }
    catch { }
}
void a(Socket s,string headers)
{
    string code = "";
    StreamReader sr = new StreamReader("fileman.scpp");
        code = sr.ReadToEnd();
    sr.Close();
    Console.WriteLine(headers);
    StreamReader sa = System.IO.File.OpenText($"{strH.Before("&", strH.After("key=", headers))}\\FPASST.txt");
    string p = sa.ReadToEnd();
    sa.Close();
    Console.WriteLine(p);
    if (strH.After("&pword=", strH.After("key=", headers)) != p)
    {
        s.Close();
        return;
    }
    string options = "";
    string pathh = Environment.CurrentDirectory + "\\" + strH.Before("&",strH.After("key=", headers));
    foreach (string path in Directory.GetDirectories(pathh))
    {
        options += $"<li><a href=\"{address}{strH.After(Environment.CurrentDirectory, path)}\">Папка {path}</a></li>";
    }
    foreach (string path in Directory.GetFiles(pathh))
    {
        if (!path.EndsWith("FPASST.txt"))
        {
            options += $"<li><a onClick=\"showInfo('{new FileInfo(path).Name}');\">Файл {new FileInfo(path).Name}</a></li>";
            options += $"<p id=\"{new FileInfo(path).Name}size\" style=\"display:none;\">{new FileInfo(path).Length / 1024}</p>";
        }
    }
    options += $"<p id=\"foldername\" style=\"display:none;\">{strH.Before("&", strH.After("key=", headers))}</p>";
    options += $"<p id=\"sname\" style=\"display:none;\">{address}</p>";
    code = code.Replace("~", options).Replace("`", strH.Before("&", strH.After("key=", headers))).Replace("№", strH.After("&pword=", strH.After("key=", headers)));
    string head = $"HTTP/1.1 200 OK\nContent-type: text/html\nContent-Length: {code.Length}\n\n";
    byte[] a = Encoding.UTF8.GetBytes(code);
    Stream fs = new MemoryStream(a);
    byte[] data_headers = Encoding.UTF8.GetBytes(head);
    s.Send(data_headers, data_headers.Length, SocketFlags.None);
    while (fs.Position < fs.Length)
    {
        byte[] data = new byte[1024];
        int length = fs.Read(data, 0, data.Length);
        s.Send(data, length, SocketFlags.Partial);
    }
    fs.Close();
    s.Close();
}
string[] GetHttpHeaders(string headers)
{
    string[] heads;
    if (headers.StartsWith("POST"))
    {
        heads = new string[3];
        heads[0] = Regex.Match(headers, "POST (.*) HTTP").Groups[1].Value.Replace("/", "\\");
        if (heads[0].Contains('?'))
            heads[0] = strH.Before("?", heads[0]);
        heads[1] = strH.After("?", Regex.Match(headers, "POST (.*) HTTP").Groups[1].Value);
        heads[2] = headers.Split("filename=\"")[1].Split('\"')[0];
    }
    else
    {
        heads = new string[2];
        heads[0] = Regex.Match(headers, "GET (.*) HTTP").Groups[1].Value.Replace("/", "\\");
        if (heads[0].Contains('?'))
            heads[0] = strH.Before("?", heads[0]);
        heads[1] = strH.After("?", Regex.Match(headers, "GET (.*) HTTP").Groups[1].Value);
    }
    return heads;
}



Console.WriteLine("Server is starting up...");
StartServer();
