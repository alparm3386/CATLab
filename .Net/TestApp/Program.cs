// See https://aka.ms/new-console-template for more information

using Com.Cat.Grpc;
using Google.Protobuf;
using Grpc.Net.Client;
using static Com.Cat.Grpc.Okapi;
using static Proto.CAT;

Console.WriteLine("Hello, World!");

//string ServerAddress = "http://localhost:50051"; // Adjust the address/port as needed
//string ServerAddress = "http://188.166.136.248:50051"; // Adjust the address/port as needed
string ServerAddress = "http://159.89.249.226:5001"; // Adjust the address/port as needed
//using var channel = GrpcChannel.ForAddress(ServerAddress);
//var client = new OkapiClient(channel);
//var request = new CreateXliffFromDocumentRequest
//{
//    FileName = "text.txt",
//    FileContent = ByteString.CopyFrom(File.ReadAllBytes("C:\\Alpar\\Test.txt")),
//    FilterContent = ByteString.Empty,
//    FilterName = "",
//    SourceLangISO6391 = "en",
//    TargetLangISO6391 = "fr"
//};
//var response = await client.CreateXliffFromDocumentAsync(request);


using var channel = GrpcChannel.ForAddress(ServerAddress);
var client = new CATClient(channel);
var request = new Proto.CreateXliffFromDocumentRequest
{
    FileName = "text.txt",
    FileContent = ByteString.CopyFrom(File.ReadAllBytes("C:\\Alpar\\Test.txt")),
    FilterContent = ByteString.Empty,
    FilterName = "",
    SourceLangISO6391 = "en",
    TargetLangISO6391 = "fr"
};
var response = await client.CreateXliffFromDocumentAsync(request);

Console.WriteLine("End.");
