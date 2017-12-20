#¬/bin/bash

echo "\n" > one-row.txt

awk -F" " '

{
	for ( i=1; i<=100000 ; i++ )
	{
		n=int(rand() * 999999999)
		while (n<100000000)
		{
			n=n+int(rand()*100000000)	
		}
		print substr( n , 0 , 10 )
	}
}

' one-row.txt > seed-file.txt
