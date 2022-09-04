// See https://aka.ms/new-console-template for more information

using SharpDedup;

var content = new byte[100000000];
if (!File.Exists("data"))
{
    File.WriteAllBytes("data", content);    
}


var paths = new string[100];
for (int i = 0; i < paths.Length; i++)
{
    paths[i] = $"data{i}";
    if (!File.Exists(paths[i]))
    {
        File.Copy("data", paths[i], true);    
    }
}


for (int i = 0; i < 10; i++)
{
    var memoryStream = new MemoryStream();
    await DeduplicationSerializer.Serialize(memoryStream, paths);
}


