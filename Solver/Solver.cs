using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;

class Solver {

    static string Decode(string input, int shift) {
        return new string(input.Select(c => (char)(c - shift)).ToArray()); //opposite function of encode, undoes char-by-char shift
    }

    static void Main() {
        using (var pipe = new AnonymousPipeClientStream(PipeDirection.In, Environment.GetEnvironmentVariable("PIPE_HANDLE")))
        using (StreamReader reader = new StreamReader(pipe)) {
            string encodedWord = reader.ReadLine(); //retrieve encoded word from pipe
            int shift = 5; //shift integer consistent with Scrambler code

            Console.Error.WriteLine($"Received Encoded Word: {encodedWord}"); //confirmation of communication success

            string decodedWord = Decode(encodedWord,shift);
            Console.WriteLine($"The decoded word is {decodedWord}"); //send decoded word back to Scrambler
        }
    }
}