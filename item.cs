datablock projectileData(QbeProjectile : hammerProjectile)
{
	directDamage = 0;
	lifeTime = 150;
	explodeOnDeath = false;
};

datablock itemData(QbeItem : hammerItem)
{
	uiName = "Qbe Hammer";
	image = QbeImage;
	colorShiftColor = "0.000000 0.700000 0.300000 1.000000";
};

datablock shapeBaseImageData(QbeImage : hammerImage)
{
	item = QbeItem;
	projectile = QbeProjectile;
	colorShiftColor = "0.000000 0.700000 0.300000 1.000000";
	stateTimeoutValue[1] = 0;
	stateTimeoutValue[2] = 0.01;
	stateTimeoutValue[3] = 0.08;
	stateTimeoutValue[4] = 0;
	stateTimeoutValue[5] = 0.08;
	stateTimeoutValue[6] = 0;
};

function QbeProjectile::onCollision(%this,%obj,%col,%fade,%pos,%normal)
{
	serverPlay3D(hammerHitSound,%pos);
	if(%col.getClassName() $= "fxDTSBrick")
	{
		if(%col.colorID != 6 && $Qbe::RoundInProgress)
		{
			if(getSimTime() - %obj.client.lastHitTime < 90)
				return;
			%obj.client.lastHitTime = getSimTime();
			%col.setColorFX(3);
			%col.schedule(50,setColorFX,0);

			%col.health -= $DefaultMinigame.multiplier;
			if(%col.health < 0)
				%col.health = 0;
			%colorRGB = getColorIDTable(%col.colorID);
			for(%i=0;%i<getWordCount(%colorRGB);%i++)
			{
				%num = getWord(%colorRGB,%i)*255;
				%str = %str @ convertRGBtoHex(%num);
			}
			%obj.client.centerPrint("<color:" @ %str @ ">" @ %col.type @ "\c6:" SPC %col.health SPC "/" SPC %col.maxHealth,2);
			if(%col.health <= 0)
			{
				%col.killBrick();
				%obj.client.score += %col.value;
				%obj.client.totalScore += %col.value;
				QbeScores.addScore(%obj.client);
			}
		}
	}
}

function QbeImage::onFire(%this,%obj,%slot)
{
	parent::onFire(%this,%obj,%slot);
	%obj.playThread(2,"armAttack");
}

function QbeImage::onStopFire(%this,%obj,%slot)
{
	%obj.playThread(2,"root");
}