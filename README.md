HearthstoneLearning jest to projekt ktory mial na celu sprawdzanie mozliwosci sieci neuralnej do nauczenia sie karcianej gry podobnej do Hearthstone TM by Blizzard Entertainment  
Implementacja logiki gry ktorej uczyla sie siec neuralna byla w znacznym stopniu uproszczona  
Implementacja sieci neuralnej znajduje sie w bibliotece "vneuralnet.dll" (moja wlasna)  

Uczenie sie przebiegalo w trybie gry przeciwko sobie od zera (oraz na bazie dannych z gier "random player" vs "random player")

Wyniki uczenia sie byly nastepujace:
- siec bardzo dobrze umiala oceniac pozycje o oczywistej przewadze jednego z graczy
- siec miala 95% winratio w grach przeciwko "random player"
- siec wciaz byla bardzo slaba w porownaniu z czlowiekiem (mna)
