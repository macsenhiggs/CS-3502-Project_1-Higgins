using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;

class Scrambler {

    static string Encode(string input, int shift) {
        return new string(input.Select(c => (char)(c + shift)).ToArray()); //simple caesar shift, increases/decreases index of char by fixed amount
    }


    static void Main() {
        string word = "operatingsystems"; //example string, free to be changed
        int shift = 5; //shift value which must be consistent between scrambler & solver in order for decoding to work
        string encodedWord = Encode(word, shift); //create scrambled word
        
        Console.WriteLine($"Encoded Word: {encodedWord}");

        using (var pipe = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable)) { //pipe process
            Process solver = new Process(); //instantiate new process
            solver.StartInfo.FileName = "dotnet";
            solver.StartInfo.Arguments = "run --project Solver"; //cmd command to run Solver project
            solver.StartInfo.RedirectStandardInput = true;
            solver.StartInfo.RedirectStandardOutput = true;
            solver.StartInfo.UseShellExecute = false;
            solver.StartInfo.Environment["PIPE_HANDLE"] = pipe.GetClientHandleAsString();
            solver.Start(); //start process after ensuring proper settings

            using (StreamWriter writer = new StreamWriter(pipe)) { //use streamwriter to write to pipe
                writer.AutoFlush = true;
                writer.WriteLine(encodedWord); //transfer encoded word across pipe
            }

            string result = solver.StandardOutput.ReadLine(); //retrieve decoded word from pipe after Solver finishes running
            Console.WriteLine($"Decoded Output: {result}");

            solver.WaitForExit(); //safe exit for Solver
        }
    }
}