# Kysymykset — Osa 2: Azure-julkaisu

Vastaa kysymyksiin omin sanoin. Lyhyet, selkeät vastaukset riittävät.

---

## Azure Blob Storage

**1.** Mitä eroa on `LocalStorageService.UploadAsync`:n ja `AzureBlobStorageService.UploadAsync`:n palauttamilla URL-arvoilla? Miksi ne eroavat?

> Vastauksesi:

LocalStorage palauttaa suhteellisen polun tyyliin /uploads/albumId/kuva.jpg, mutta AzureBlobStorage palauttaa täyden blob-URL:n https://...blob.core.windows.net/..

Ne eroaa koska lokaalissa tiedosto tarjoillaan oman webapin wwwroot-kansiosta, Azuressa tiedosto asuu erillisessä Blob Storagessa

---

**2.** `AzureBlobStorageService` luo `BlobServiceClient`:n käyttäen `DefaultAzureCredential()` eikä yhteysmerkkijonoa. Mitä etua tästä on? Mitä `DefaultAzureCredential` tekee eri ympäristöissä?

> Vastauksesi:

Iso etu on se ettei connection stringejä tai avaimia tarvitse upottaa koodiin. DefaultAzureCredential yrittää automaattisesti sopivat tavat: lokaalisti esim az login tai VS-kirjautuminen, Azuressa Managed Identity.

---

**3.** Blob Container luodaan `--public-access blob` -asetuksella. Mitä tämä tarkoittaa: mitä pystyy tekemään ilman tunnistautumista, ja mikä vaatii Managed Identityn?

> Vastauksesi:

`--public-access blob` tarkoittaa että blobien luku URL:lla onnistuu ilman kirjautumista. Kirjoitus ja poisto vaatii silti oikeudet, ne tulee Managed Identity ja RBAC kautta.

---

## Application Settings

**4.** Application Settings ylikirjoittavat `appsettings.json`:n arvot. Selitä tämä mekanismi: miten se toimii ja miksi se on hyödyllistä eri ympäristöjä varten?

> Vastauksesi:

ASP.NET Coren konfiguraatio yhdistää monta lähdettä yhteen, ja viimeiseksi/ylemmällä prioriteetilla tuleva arvo voittaa. Siksi Azure Application Settings voi ylikirjoittaa saman avaimen `appsettings.json`:sta ilman koodimuutosta.

Tämä on kätevä koska dev test prod voi käyttää eri arvoja samalla binäärillä

---

**5.** Application Settingsissa käytetään `Storage__Provider` (kaksi alaviivaa), mutta koodissa luetaan `configuration["Storage:Provider"]` (kaksoispiste). Miksi?

> Vastauksesi:

Azure App Settings ei tue : merkkiä avaimessa samalla tavalla, joten siellä käytetään __. .NET muuntaa sen automaattisesti hierarkiaksi, eli `Storage__Provider` == `Storage:Provider`.

---

**6.** Mitkä konfiguraatioarvot soveltuvat Application Settingsiin, ja mitkä eivät? Anna esimerkki kummastakin tässä tehtävässä.

> Vastauksesi:

Application Settingsiin sopii ei-salaiset ympäristökohtaiset arvot, esim `Storage__Provider=azure` ja `Storage__AccountName=`.

Ei kannata laittaa oikeita salaisuuksia kuten ModerationService:ApiKey.

---

## Managed Identity ja RBAC

**7.** Selitä omin sanoin: mitä tarkoittaa "System-assigned Managed Identity"? Mitä tapahtuu tälle identiteetille, jos App Service poistetaan?

> Vastauksesi:

System-assigned Managed Identity on app servicen oma identiteetti jonka Azure luo ja hallitsee. sitä ei tarvitse itse kierrättää tai säilyttää.

Kun App service poistetaan myös identiteetti poistuu automaattisesti

---

**8.** App Servicelle annettiin `Storage Blob Data Contributor` -rooli Storage Accountin tasolle — ei koko subscriptionin tasolle. Miksi tämä on parempi tapa? Mikä periaate tähän liittyy?

> Vastauksesi:

Se on parempi koska oikeus rajataan pienimpään tarvittavaan scopeen. jos rooli annettaisiin subscriptioniin asti vahinkoalue olisi paljon suurempi jos sovellus tai tunnus kompromisoituu.

---


