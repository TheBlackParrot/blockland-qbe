function getRankFormat(%number)
{
	if(strLen(%number) > 2)
	{
		%ending = getSubStr(%number,strLen(%number)-2,strLen(%number));
		switch(%ending)
		{
			case 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19:
				return %number @ "th";
		}
		%ending = getSubStr(%ending,1,1);
	}
	else
		%ending = %number;
	switch(%ending)
	{
		case 1:
			return %number @ "st";
		case 2:
			return %number @ "nd";
		case 3:
			return %number @ "rd";
		case 4 or 5 or 6 or 7 or 8 or 9 or 0:
			return %number @ "th";
	}
}

function formatNumber(%num)
{
	%len = strLen(%num);
	if(%len < 4)
		return %num;
	if(striPos(%num,"-") == 0)
	{
		%negative = "-";
		%num = getSubStr(%num,1,%len);
		%len--;
	}
	while(%len > 3)
	{
		%formatted = "," @ getSubStr(%num,%len-3,%len) @ %formatted;
		%num = getSubStr(%num,0,%len-3);
		%len -= 3;
	}
	return %negative @ %num @ %formatted;
}

function mRound(%number)
{
	if(%number < 0.5)
		return mFloor(%number);
	if(%number >= 0.5)
		return mCeil(%number);
	return -1;
}