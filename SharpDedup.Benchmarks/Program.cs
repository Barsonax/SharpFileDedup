// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using SharpDedup.Console;

var summary = BenchmarkRunner.Run<BasicBenchmark>();