Console.WriteLine("enter expected Path:");
var path = Console.ReadLine();

var directories = GetDirectories(path);

var files = new List<string>();

foreach (var d in directories)
{
    files.AddRange(Directory.GetFiles(d).Where(file => file.EndsWith("Controller.cs")));
}

var permissions = new List<string>();

foreach (var file in files)
{
    var newLines = new List<string>();
    var changed = false;
    var haveNamespace = false;

    foreach (var line in File.ReadAllLines(file))
    {
        if (line.Contains("[Permission("))
        {
            continue;
        }

        if (line.Contains("using AppleID.Application.Security;"))
        {
            haveNamespace = true;
        }

        if (line.Contains("Controller") && line.Contains("public") && line.Contains("class"))
        {

            var controllerName = line.Trim().Split(" ")[2].Replace("Controller", "");
            var permission = $"Manage{controllerName}";

            newLines.Add($"[Permission(\"{permission}\")]");
            permissions.Add(permission);
            changed = true;
        }
        if ((line.Contains("ActionResult")) && line.Contains("public"))
        {
            var actionName = line.Split("(").First().Split(" ").Last();
            var permission = actionName;

            newLines.Add($"[Permission(\"{permission}\")]");
            permissions.Add(permission);
            changed = true;
        }

        newLines.Add(line);
    }

    if (changed)
    {
        if (!haveNamespace)
        {
            newLines.Insert(0, "using AppleID.Application.Security;");
        }
        File.WriteAllLines(file, newLines);
    }
}

var permissionsPart = new List<string>();

var index = 0;
var lastRoot = 0;
foreach (var item in permissions.DistinctBy(x => x))
{
    index++;

    var isManage = item.Contains("Manage");
    var parentId = isManage ? "null" : lastRoot.ToString();
    if (isManage)
    {
        lastRoot = index;

        if (index > 0)
        {
            permissionsPart.Add("#endregion");
            permissionsPart.Add(Environment.NewLine);
        }

        permissionsPart.Add($"#region {item.Replace("Manage", "")}");
        permissionsPart.Add(Environment.NewLine);
    }

    permissionsPart.Add($"new Permission {Environment.NewLine}");
    permissionsPart.Add($"{{ {Environment.NewLine}");
    permissionsPart.Add($"Id = {index}, {Environment.NewLine}");
    permissionsPart.Add($"UniqueName = \"{item}\", {Environment.NewLine}");
    permissionsPart.Add($"ParentId = {parentId} {Environment.NewLine}");
    permissionsPart.Add($"}}, {Environment.NewLine}");
}

permissionsPart.Add("#endregion");

//File.Create("permissions.txt").Close();
File.WriteAllLines("permissions.txt", permissionsPart);

List<string> GetDirectories(string path)
{

    var directories1 = new List<string>();

    directories1.Add(path);

    var newDiectory = new List<string>();
    newDiectory.AddRange(directories1);

    var tempDirectory = new List<string>();

    while (true)
    {
        foreach (var item in newDiectory)
        {
            tempDirectory.AddRange(Directory.GetDirectories(item));
        }

        if (tempDirectory.Count == 0) break;

        newDiectory.Clear();
        newDiectory.AddRange(tempDirectory);
        directories1.AddRange(tempDirectory);
        tempDirectory.Clear();
    }

    return directories1;
}