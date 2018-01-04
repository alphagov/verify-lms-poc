#¬/bin/bash

awk -F" " '

BEGIN {

        firstn_cnt = read_file_into_array( "forenames-gender.txt" , firstn )
        surn_cnt = read_file_into_array( "surnames.txt" , surn )
        dom_cnt = read_file_into_array( "domains.txt" , dom )
        post_cnt = read_file_into_array( "postcodes-and-town.txt" , post )
        street_cnt = read_file_into_array( "streets-fixed.txt" , street )

	ASCII="ABCDEFGHIJKLMNOPQRSTUVWXYZD"
        NUMBER="0123456789"

	printf "\"%s\",\"%s\",\"%s\",\"%s\",\"%s%s\",\"%s\",\"%s%s\",\"%s%s%s\",\"%s\",\"%s%s%s\",\"%s\"\n" ,  "ID" , "FirstName" , "MiddleName" , "SurName" , "Address1" , "" , "Address2" , "PostCode", ""  , "DateOfBirth","","","PhoneNumber", "Email","","","Gender"


}


{


	_fn = firstn[ notzero(substr($0,4,6)) % firstn_cnt]
	_gen = substr( _fn , length(_fn),1)
	_fn = rtrim( substr( _fn , 0 , length(_fn)-2) )

	_mn = firstn[ notzero(substr($0,2,6)) % firstn_cnt]

	_mn = substr( _mn , 0, length(_mn)-2)
	

	_ln = surn[ notzero(substr($0,3,6)) % surn_cnt]
	_do = dom[ notzero(substr($0,4,6)) % dom_cnt]
	_po = post[ notzero(substr($0,2,7)) % post_cnt]
	_st = street[ notzero(substr($0,2,8)) % street_cnt ]


	_dob_d = notzero(substr($0,9,2))%30
	_dob_m = notzero(substr($0,6,2))%12	
	_dob_y = 2009 - (notzero(substr($0,4,3))%90)

	if ((_dob_m == 2) && (_dob_d > 28)) _dob_d=_dob_d-9

	_pn = "07" substr($0,1,9)

	_hn = int((int( substr($0,5,3))+1)/4)+1


	_pcnv = substr($0,2,2)


	_pca1 = substr($0,3,2)

	_pca2 = substr($0,6,2)
	

	_pcn = notzero(_pcnv % 29) " " substr($0,1,1) substr( ASCII , notzero( _pca1%26) , 1 ) substr( ASCII , notzero(_pca2%26) , 1 )
	

	split(substr(_po,0,length(_po)-1),_ad,"\t");

	if (int(substr($0,9,1)) < 4) 
	{
		 _mn=""
	}

	if (_gen == "F")
	{
		_gen = "Female"
	}
	else
	{
		_gen = "Male"
	}

	printf "\"%s\",\"%s\",\"%s\",\"%s\",\"%s %s\",\"%s\",\"%s%s\",\"%s/%s/%s\",\"%s\",\"%s.%s@%s\",\"%s\"\n" , NR , _fn , _mn , _ln , _hn , _st ,_ad[2] , _ad[1], _pcn  , _dob_y,_dob_m,_dob_d,_pn,tolower(_fn),tolower(_ln),tolower(_do), _gen


}

function notzero(s) { if (s==0) { return 1;} else {return int(s);} }
function ltrim(s) { sub(/^[ \t\r\n]+/, "", s); return s }
function rtrim(s) { sub(/[ \t\r\n]+$/, "", s); return s }
function trim(s) { return rtrim(ltrim(s)); }

function read_file_into_array(file, array ,status, record, count ) {
   count  = 0;
   while (1) {
      status = getline record < file
      if (status == -1) {
         print "Failed to read file " file;
         exit 1;
      }
      if (status == 0) break;
      array[++count] = record;
   }
   close(file);
   return count
}

' seed-file.txt
