# Kysymykset — Osa 1: Lokaali kehitys

Vastaa kysymyksiin omin sanoin. Lyhyet, selkeät vastaukset riittävät — tarkoitus on osoittaa, että olet ymmärtänyt konseptit.

---

## Clean Architecture

**1.** Selitä omin sanoin: mitä tarkoittaa, että `UploadPhotoUseCase` "ei tiedä" tallennetaanko kuva paikalliselle levylle vai Azureen? Näytä koodirivit, jotka osoittavat tämän.

> Vastauksesi:

UploadPhotoUseCase ei tunne sitä tallennetaanko kuva levylle vai Azureen, koska se kutsuu vain rajapintaa.
Koodirivit:
- StarterCode/GalleryApi/GalleryApi.Application/UseCases/Photos/UploadPhotoUseCase.cs rivi 49: imageUrl = await _storageService.UploadAsync()
- StarterCode/GalleryApi/GalleryApi.Domain/Interfaces/IStorageService.cs rivi 12: Task<string> UploadAsync()

---

**2.** Miksi `IStorageService`-rajapinta on määritelty `GalleryApi.Domain`-kerroksessa, mutta `LocalStorageService` on `GalleryApi.Infrastructure`-kerroksessa? Mitä hyötyä tästä jaosta on?

> Vastauksesi:

Rajapinta on Domainissa koska se kuuluu sovelluksen ytimeen (mitä pitää pystyä tekemään), mutta LocalStorageService on Infraa koska se on tekninen tapa toteuttaa se. Hyöty on se että voit vaihtaa teknologian ilman että business-logiikka menee uusiksi.

---

**3.** Testit käyttävät `Mock<IAlbumRepository>`. Mitä mock-objekti tarkoittaa, ja miksi Clean Architecture tekee tämän testaustavan mahdolliseksi?

> Vastauksesi:

mock-objekti on feikki toteutus rajapinnasta, sille voi itse määrittää mitä se palauttaa testissä. Clean Architecture mahdollistaa tämän koska use case riippuu rajapinnasta eikä suoraan tietokannasta.

---

## Salaisuuksien hallinta

**4.** Kovakoodattu API-avain on ongelma, vaikka repositorio olisi yksityinen. Selitä kaksi eri syytä miksi.

> Vastauksesi:

syyt:
1. Yksityisessä repossakin avain leviää kaikille joilla on pääsy, myös entisille jäsenille jos käyttöoikeuksia ei siivota hyvin
2. avain voi päätyä CI-lokeihin, varmuuskopioihin tai screenshotteihin ja silloin se on käytännössä vuotanut

---

**5.** Riittääkö se, että poistat kovakoodatun avaimen myöhemmässä commitissa? Perustele vastauksesi.

> Vastauksesi:

Ei riitä. git-historiaan jää aiempi commit, ja avain löytyy sieltä edelleen vaikka uusin versio ei sitä näyttäisi.

---

**6.** Minne User Secrets tallennetaan käyttöjärjestelmässä? (Mainitse sekä Windows- että Linux/macOS-polut.) Miksi tämä sijainti on turvallinen?

> Vastauksesi:

Windows: `%APPDATA%\\Microsoft\\UserSecrets\\<UserSecretsId>\\secrets.json`
Linux/macOS: `~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`

Tämä on turvallisempi koska tiedosto ei ole projektikansiossa eikä mene git commitiin automaattisesti

---

## Options Pattern ja konfiguraatio

**7.** Mitä hyötyä on `IOptions<ModerationServiceOptions>`:n käyttämisestä verrattuna siihen, että luetaan arvo suoraan `IConfiguration`-rajapinnalta (`configuration["ModerationService:ApiKey"]`)?

> Vastauksesi:

`IOptions<ModerationServiceOptions>` antaa tyyppiturvallisuuden, IntelliSensen ja selkeämmän testattavuuden. Jos käyttää suoraa string-avainta kirjoitusvirhe huomataan vasta ajossa.

---

**8.** ASP.NET Core lukee konfiguraation useista lähteistä prioriteettijärjestyksessä. Listaa lähteet korkeimmasta matalimpaan ja selitä, mikä arvo lopulta käytetään, kun sama avain on sekä `appsettings.json`:ssa että User Secretsissä.

> Vastauksesi:

Tässä tehtävässä prioriteetti oli:
1. User Secrets
2. appsettings.Development.json
3. appsettings.json

Jos sama avain on sekä appsettingsissä että User Secretsissä, User Secrets voittaa ja sitä arvoa käytetään

---

**9.** `DependencyInjection.cs`:ssä valitaan tallennustoteutus näin:

```csharp
var provider = configuration["Storage:Provider"] ?? "local";
if (provider == "azure")
    services.AddScoped<IStorageService, AzureBlobStorageService>();
else
    services.AddScoped<IStorageService, LocalStorageService>();
```

Miksi käytetään konfiguraatioarvoa `env.IsDevelopment()`-tarkistuksen sijaan? Mitä haittaa olisi `if (env.IsDevelopment()) { käytä lokaalia }`-lähestymistavassa?

> Vastauksesi:

Koska ympäristö ei suoraan kerro missä data pitää säilyttää. Voit hyvin ajaa production-ympäristössäkin local-storagea testiin, tai devissä azurea integraatiotestiin.

`env.IsDevelopment()` tekisi tästä liian jäykän ja piilottaisi oikean valinnan konfiguraation sijasta.

---

## Tiedostotallennus

**10.** Kun lataat kuvan, `imageUrl`-kentän arvo on `/uploads/abc123-..../photo.jpg`. Miten tähän URL:iin pääsee selaimella? Mihin koodiin tämä perustuu?

> Vastauksesi:

Selaimella pääsee suoraan URL:iin lisäämällä hostin eteen, esim `https://localhost:1234/uploads/jne`.
Tämä perustuu siihen että LocalStorageService palauttaa URL:n ja WebApi ottaa staattiset tiedostot käyttöön

---

**11.** Mitä tapahtuu jos yrität ladata tiedoston jonka MIME-tyyppi on `application/pdf`? Missä tiedostossa ja millä koodirivillä tämä käyttäytyminen on määritelty?

> Vastauksesi:

application/pdf hylätään validoinnissa ja tulos on epäonnistunut (`Result.Failure`).
Määrittely on tiedostossa StarterCode/GalleryApi/GalleryApi.Application/UseCases/Photos/UploadPhotoUseCase.cs:
- sallitut tyypit rivillä 15 (`AllowedContentTypes`)
- tarkistus rivillä 37 (`if (!AllowedContentTypes.Contains(request.ContentType))`)

HTTP-tasolla controller mapittaa tämän 400 BadRequest-vastaukseksi (`StarterCode/GalleryApi/GalleryApi.WebApi/Controllers/PhotosController.cs` rivi 55)

---

**12.** `DeletePhotoUseCase` poistaa tiedoston kutsumalla `_storageService.DeleteAsync(photo.FileName, photo.AlbumId)` — ei `photo.ImageUrl`:lla. Miksi?

> Vastauksesi:

Koska ImageUrl on asiakkaalle näytettävä osoite, ei välttämättä storage-järjestelmän sisäinen avain. Poisto tarvitsee deterministisen nimen, siksi käytetään FileName + AlbumId.

Azuressa tämä on tärkeä koska URL voi muuttua mutta blobin nimi pysyy
