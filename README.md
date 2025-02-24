02/28/2025

Hello! This is my project on multi-threading and inter-process communication (IPC).
There are two programs contained in this project.

Project A: Bank interface
    Allows safe, synchronized deposits and withdrawals to/from personal bank accounts using Threads and Mutexes as well as transfers between accounts with deadlock detection and mitigation.

    How to run:
        Open folder Bank in VS Code and run Bank.cs. 
        
        The program will simulate examples of accounts withdrawing and depositing funds synchronously, as well as demonstrate two accounts trying to transfer funds to each other at the same time, which can cause deadlock, and the resolution of said deadlock.

Project B: Word Encoding and Decoding using IPC
    Demonstration of the functionality of pipes in C# for communication between actively running processes. Uses a simple Caesar cipher for demonstration purposes.

    How to run:
        Open folder Scrambler in VS Code and run Scrambler.cs.

        The program will take an example word, in this case "operatingsystems", and shift its characters individually by 5. The program will then start running Solver.cs and create a pipe between the two and then send the encoded word through the pipe for Solver to decode. Solver will then backshift the encoded word's characters individually by 5, resulting in our original word. This word will then be sent back down the pipe for Scrambler to receive and print.