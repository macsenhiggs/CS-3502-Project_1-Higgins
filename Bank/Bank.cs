using System;
using System.Collections.Generic;
using System.Threading;

class Banking {
    public static Dictionary<int, int> accounts = new Dictionary<int, int>() //instantiate 10x2 int-int dictionary for accounts
    {
            //first column is ID#, second column is balance
            {1001, 5000},
            {1002, 68},
            {1003, 1250},
            {1004, 7989},
            {1005, 0},
            {1006, 4444},
            {1007, 7000},
            {1008, 654},
            {1009, 10000},
            {1010, 5638},
    };

    public static Dictionary<int, Mutex> accountLocks = new Dictionary<int, Mutex>() //instantiate 10x2 int-mutex dictionary, 1 mutex per account 
    {
            {1001, new Mutex()},
            {1002, new Mutex()},
            {1003, new Mutex()},
            {1004, new Mutex()},
            {1005, new Mutex()},
            {1006, new Mutex()},
            {1007, new Mutex()},
            {1008, new Mutex()},
            {1009, new Mutex()},
            {1010, new Mutex()},
    };

    public static Mutex singleMutex = new Mutex();  //mutex instantiation 

    public static void printAccounts() { //simple method to print all account balances
        Console.WriteLine("\nAccount balances: ");
        foreach (var myAccount in accounts) {
            Console.WriteLine($"ID {myAccount.Key}: ${myAccount.Value}");
        }
        Console.WriteLine("");
    }

    static void Main(string[] args) {
        Console.WriteLine("Welcome to K&B Bank!");
        
        Thread[] threads = new Thread[accounts.Count]; //1 thread per account

        for (int i = 0; i < threads.Length; i++) { //instantiate all threads for each account
            int userID = 1001 + i;
            threads[i] = new Thread(() => SingleAccessAccount(userID));
            threads[i].Start();
        }

        foreach (Thread myThread in threads) {
            myThread.Join(); //wait until all threads finish
        }

        printAccounts();

        //example for transfers between 2 accounts which can create deadlock and deadlock resolution
        Thread t1 = new Thread(() => Transfer(1001, 1002, 200) ); //$200 from 1001 to 1002
        Thread t2 = new Thread(() => Transfer(1002, 1001, 300) ); //$300 from 1002 to 1001

        //start threads
        t1.Start();
        t2.Start();

        //wait for threads to finish
        t1.Join();
        t2.Join();

        printAccounts();

    }

    static void SingleAccessAccount(int userID) { //method for individual deposits/withdrawals
        Random rand = new Random(); //used for adding/taking random amount of $
        int amount = rand.Next(100, 1000);
        bool deposit = rand.Next(2) == 0; //0 for deposit, 1 for withdrawal

        singleMutex.WaitOne(); //mutex waits for signal from thread

        try {
            if (!accounts.ContainsKey(userID)) { //edge case for theoretical model with user input
                Console.WriteLine($"User {userID} could not be found.");
                return;
            }

            if (deposit) { //deposits will always be accepted
                accounts[userID] += amount;
                Console.WriteLine($"User {userID} deposited ${amount}. New balance: ${accounts[userID]}");  
            }
            else { //withdrawals will not always work, requiring check before continuing
                if (accounts[userID] >= amount) {
                    accounts[userID] -= amount;
                    Console.WriteLine($"User {userID} withdrew ${amount}. New balance: ${accounts[userID]}");  
                }
                else {
                    Console.WriteLine($"User {userID} tried to withdraw ${amount} but has insufficient funds");  
                }
            }
        }
        finally {
            singleMutex.ReleaseMutex(); //release mutex now that process has completed
        }
    }

    static void Transfer(int fromAccount, int toAccount, int amount) { //method for transfer from 1 account to another
        Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} trying to transfer ${amount} from {fromAccount} to {toAccount}"); //announcement of transfer start
        while (true) { //repeat indefinitely until transfer succeeds
            accountLocks[fromAccount].WaitOne(); //have mutex wait for thread
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} locked account {fromAccount}"); //confirmation of account locking
            
            if (accountLocks[toAccount].WaitOne(TimeSpan.FromMilliseconds(500))) { //if receiving account connects within 0.5s, continue
                try {
                    if (accounts[fromAccount] >= amount) { //same check as withdrawal to ensure sufficient funds
                        accounts[fromAccount] -= amount;
                        accounts[toAccount] += amount;
                        Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} successfully transferred ${amount} from {fromAccount} to {toAccount}");
                    }
                    else {
                        Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} failed to transfer: insufficient funds");
                    }
                    return; //exit loop
                }
                finally {
                    //release both mutexes
                    accountLocks[toAccount].ReleaseMutex();
                    accountLocks[fromAccount].ReleaseMutex();
                }
            }
            else { //threads stalled out for >0.5s, likely because of deadlock
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} timed out waiting for account {toAccount}, retrying..."); //announce failure
                accountLocks[fromAccount].ReleaseMutex(); //release giving account's mutex
                Random rand = new Random();
                Thread.Sleep(rand.Next(100,300)); //wait 0.1-0.3s then try again, which usually resolves deadlock
            }
        }
    }
}