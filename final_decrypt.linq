<Query Kind="Program">
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

public class Example
{
	public static void Main(){
		string password = "";
		string origTickcount = "";	//This variable to store the actual Tickcout value
		bool found = false;
		int startingTickcount = 21666468; //Seed found from the first encrypted file 21666468
		int biggerRangeSize = (Int32.MaxValue - startingTickcount < startingTickcount) ? startingTickcount : Int32.MaxValue - startingTickcount;
		int downIndex = 0, upIndex = 0;
		string charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890*"; //Charset used by Whiterose
		string fileToBruteForcePath = "<encrypted file path>"; //Encrypted whiterose file
	
		// Create the token source.
      	CancellationTokenSource cts = new CancellationTokenSource();		
		Enumerable.Range(1,biggerRangeSize).AsParallel().AsOrdered().WithCancellation(cts.Token).ForAll((index)=>{	
		
			// Increment Branch
			if (!found && (float)startingTickcount + (float)index <= (float)Int32.MaxValue){
				if (TryFileDecrypt(charset, (startingTickcount+index), fileToBruteForcePath, ref password)){ 
					found = true;
					origTickcount = (startingTickcount + index).ToString(); //This is the final Tickcount value
					Console.WriteLine("The original tick count found! Here it is: " + origTickcount);
					Console.WriteLine("Finish decryption!");
					cts.Cancel(); //Stop the program
				}	
				upIndex++;		
			}
			
			// Decrement Branch
			if (!found && startingTickcount - index >=0){
				if (TryFileDecrypt(charset, startingTickcount - index, fileToBruteForcePath, ref password)){
					found = true;
					origTickcount = (startingTickcount - index).ToString(); //This is the final Tickcount value
					Console.WriteLine("The original tick count found! Here it is: " + origTickcount);
					Console.WriteLine("Finish decryption!");
					cts.Cancel(); //stop the program
				}
				downIndex--;								
			}
		});
	}
	
	public static bool TryFileDecrypt(string charset, int index, string fileToBruteForce, ref string password){
		string randomToken = generateToken(charset, 48, index); 
		byte[] passwordBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(randomToken)); 		
		byte[] decryptedFile = DecryptFile(fileToBruteForce, passwordBytes);
		string originalFilename = checkDecryptedFile(decryptedFile);
		
		if (originalFilename != ""){
			password = randomToken;
			Console.WriteLine("Here is the AES key: " + randomToken);
			Console.WriteLine("Here is the original filename: " + originalFilename);
			var decryptedFilePath = "<folder path>" + originalFilename;
			File.WriteAllBytes(decryptedFilePath, decryptedFile);
			Console.WriteLine("Decrypted File has been written to: " + decryptedFilePath);
			return true;
		}
		else{
			return false;
		}
	}

	public static string generateToken(string charset, int length, int seed){
		StringBuilder stringBuilder = new StringBuilder();
		Random random = new Random(seed);
		while (0 < length--){
			stringBuilder.Append(charset[random.Next(charset.Length)]);
		}
		return stringBuilder.ToString();	//This would return the 48 character 		
	}
	
	public static byte[] DecryptFile(string Path, byte[] passwordBytes){
		try{
			byte[] array = File.ReadAllBytes(Path);
			byte[] array2 = AESDecrypt(array, passwordBytes);	
			return array2;
		}
		catch (Exception){
			return null;
		}
	}
	
	public static string checkDecryptedFile(byte[] array){
		try{					
			byte[] last256 = new byte[256];
			Array.Copy(array, array.Length - 256, last256, 0, 256);
			if (last256[0] == 0){
				return "";
			}
			bool primerCeroEncontrado = false;		//Primer Cero Encontrado == First Zero Found
			int index = 0;
			for (int i = 1; i < last256.Length; i++){
				if (!primerCeroEncontrado && last256[i] == 0){
					primerCeroEncontrado = true;
					index = i;
				}
				if (primerCeroEncontrado && last256[i] !=0){
					return "";
				}
			}
			byte[] originalFilename = new byte[index];
			Array.Copy(array, array.Length - 256, originalFilename, 0, index);
			return System.Text.Encoding.UTF8.GetString(originalFilename);
		}
		catch (Exception){
			return "";
		}
	}
	
	static byte[] AESDecrypt(byte[] cipherText, byte[] passwordBytes){		
		try{
			byte[] salt =new byte[] 
			{1,2,3,4,5,6,7,8};	
			byte[] plaintext = null; // Declare the string used to hold the decrypted text.		
			using (RijndaelManaged rijAlg = new RijndaelManaged()){
				rijAlg.KeySize = 256;
				rijAlg.BlockSize = 128;
				Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(passwordBytes,salt,5000);
				rijAlg.Key = rfc2898DeriveBytes.GetBytes(rijAlg.KeySize / 8);
				rijAlg.IV = rfc2898DeriveBytes.GetBytes(rijAlg.BlockSize / 8);
				rijAlg.Mode = CipherMode.CBC;
				ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
	               // Create the streams used for decryption.
	            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
	        	{
	            	using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
	            	{
						using (var output = new MemoryStream()){
							csDecrypt.CopyTo(output);
							plaintext = output.ToArray();
						}
	                }					
	            }				
			}
			return plaintext;
		}
		catch (Exception){
		}
		return null;
	}	
}
