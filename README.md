Here's a short write-up of a ransomware decryptor I built while analysing the Whiterose ransomware. 
## Description
Whiterose is a .NET binary and can be decompiled with dnSpy. However, it is obfuscated with confuserEx.

## Deobfuscation
Follow these steps to deobfuscate the program:
  - Go to the program's entry point
  - Set a breakpoint at gchandle.Free() 
  - Start debugging the program 
  - Once the process has been interrupted by the breakpoint, observe that there is a new module named "koi"
  - Dump this newly loaded module "koi" from memory to disk 

## Analysing the source code
In the main function, a 48 character AES key is calculated using the `RandomString` function. This `RandomString` function is also used to calculate `<random text>` used in the encrypted file name

#### RandomString function
The RandomString function uses the .NET Random class which is a pseudorandom numbers generator. This is risky as pseudorandom number generators are cryptographically insecure. This makes it easy to recover the AES encryption key.
The .NET random class here is called without any arguments, hence implying that the program is using the system tick count as seed. (Tickcount = a signed integer of how many milliseconds after the system boots)

## Running the ransomware in a VM
- Run noriben.py then run the ransomware executable
- Analyse the logs generated and find the first encrypted file (in this case delphi_filter.txt)
Since the tick count used to generate the first encrypted file should be a value close to the tick count used to generate the AES key, finding the tickcount used for this file will help us to brute force the tick count of the AES key. We can do so using untwister.

## Untwister
A multi-threaded seed recovery tool for common pseudorandom number generators. Using untwister, the tick count for the first encrypted file (delphi_filter.txt) is 21666468. 

## Thought process
Knowing that the AES key's tick count value should be a value close to this seed, I can start brute-forcing the tick count using an integer counter. For every tick count value, I will generate a 48 character key to try to decrypt the file.  Additionally, from the source code analysis, the program appends the original filename to the end of the ciphered file for easy restoration after decryption. Hence, if I can get the original file name delphi_filter.txt at the end of the file, I can confirm that the file has been decrypted successfully.

## Usage
1. Download linqpad to run the decryptor code
2. Under the `Main` function, change the fileToBruteForcePath to the encrypted whiterose file's path
3. Under `TryFileDecrypt` function, change the decryptedFilePath to your preferred directory
4. Run the program and obtain the AES key!