// See https://aka.ms/new-console-template for more information

using Com.Cat.Grpc;
using Google.Protobuf;
using Grpc.Net.Client;
using Proto;
using static Com.Cat.Grpc.Okapi;
using static Proto.CAT;

Console.WriteLine("Hello, World!");

string ServerAddress = "http://localhost:50051"; // Adjust the address/port as needed
//string ServerAddress = "http://188.166.136.248:50051"; // Adjust the address/port as needed
//string ServerAddress = "http://159.89.249.226:5001"; // Adjust the address/port as needed

//using var channel = GrpcChannel.ForAddress("http://localhost:50051");
//var client = new OkapiClient(channel);
//var request2 = new Com.Cat.Grpc.CreateXliffFromDocumentRequest
//{
//    FileName = "text.txt",
//    FileContent = ByteString.CopyFrom(File.ReadAllBytes("C:\\Alpar\\Test.txt")),
//    FilterContent = ByteString.Empty,
//    FilterName = "",
//    SourceLangISO6391 = "en",
//    TargetLangISO6391 = "fr"
//};
//var response = await client.CreateXliffFromDocumentAsync(request2);


//var channel2 = GrpcChannel.ForAddress(ServerAddress);
//var client2 = new CATClient(channel);

//var request1 = new TMExistsRequest { TmId = "1/_1_en_fr_marketing" };
//var response1 = await client2.TMExistsAsync(request1);


var channel = GrpcChannel.ForAddress(ServerAddress);
var client3 = new CATClient(channel);
var request = new Proto.CreateXliffFromDocumentRequest
{
    FileName = "text.txt",
    FileContent = ByteString.CopyFrom(File.ReadAllBytes("C:\\Alpar\\Test.txt")),
    FilterContent = ByteString.Empty,
    FilterName = "",
    SourceLangISO6391 = "en",
    TargetLangISO6391 = "fr"
};
var response3 = await client3.CreateXliffFromDocumentAsync(request);

Console.WriteLine("End.");
