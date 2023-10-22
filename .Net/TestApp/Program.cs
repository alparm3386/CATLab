// See https://aka.ms/new-console-template for more information
using TestApp;

Console.WriteLine("Hello, World!");

var okapiConnector = new OkapiConnector();
var xliffContent = okapiConnector.CreateXliffFromDocument("test.txt", File.ReadAllBytes("C:\\Alpar\\test.txt"), "", null!, "en", "fr");
Console.WriteLine(xliffContent);


