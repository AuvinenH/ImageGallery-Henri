# Kysymykset — Osa 3: Key Vault ja Infrastructure as Code

Vastaa kysymyksiin omin sanoin. Lyhyet, selkeät vastaukset riittävät.

---

## Key Vault

**1.** Miksi `ModerationService:ApiKey` tallennettiin Key Vaultiin eikä Application Settingsiin? Mitä lisäarvoa Key Vault tuo Application Settingsiin verrattuna?

> Vastauksesi:

Koska se on oikea salaisuus eikä pelkkä konfiguraatio. Application Settingsissä arvo näkyy aika helposti portaalissa, Key Vaultissa taas saat salauksen versionnin auditoinnin ja tiukemman RBAC hallinnan.

Key Vault on turvallisempi paikka avaimille

---

**2.** Key Vault -salaisuuden nimi on `ModerationService--ApiKey` (kaksi väliviivaa), mutta koodissa se luetaan `configuration["ModerationService:ApiKey"]` (kaksoispiste). Miksi käytetään `--`?

> Vastauksesi:

Key Vaultin salaisuuden nimessä ei voi käyttää : eikä __, siksi käytetään `--`. Key Vault provider mapittaa sen automaattisesti konfiguraatioavaimeksi ModerationService:ApiKey.

---

**3.** `Program.cs`:ssä Key Vault lisätään konfiguraatiolähteeksi `if (!string.IsNullOrEmpty(keyVaultUrl))`-ehdolla. Miksi tämä ehto on tärkeä? Mitä tapahtuisi ilman sitä?

> Vastauksesi:

Ehto on tärkeä koska lokaalisti KeyVault:VaultUrl voi olla tyhjä. Ilman ehtoa sovellus yrittäisi aina yhdistää Key Vaultiin ja kaatuisi/antaisi virheitä myös tilanteissa missä sitä ei edes tarvita.

---

**4.** Kun sovellus on käynnissä Azuressa, konfiguraation prioriteettijärjestys on: Key Vault → Application Settings → `appsettings.json`. Selitä millä arvolla `ModerationService:ApiKey` lopulta ladataan — ja käy läpi jokainen askel siitä, miten arvo päätyy sovelluksen `IOptions<ModerationServiceOptions>`:iin.

> Vastauksesi:

Lopullinen arvo tulee Key Vaultista jos avain löytyy sieltä. järjestys on näin:
1. Program.cs lukee KeyVault:VaultUrl ja lisää Key Vaultin konfiguraatiolähteeksi
2. provider hakee salaisuuden ModerationService--ApiKey ja mapittaa sen avaimeen `ModerationService:ApiKey`
3. builder.Services.Configure<ModerationServiceOptions>(GetSection("ModerationService")) sitoo arvon options-olioon
4. `IOptions<ModerationServiceOptions>` injektoidaan clientille ja ApiKey tulee sieltä

Koska prioriteetti on Key Vault > App Settings > appsettings, Key Vaultin arvo voittaa jos sama avain on muuallakin

---

**5.** Mitä eroa on `Key Vault Secrets User` ja `Key Vault Secrets Officer` -roolien välillä? Miksi annettiin nimenomaan `Secrets User`?

> Vastauksesi:

`Secrets User` saa lukea salaisuuksia, `Secrets Officer` voi hallita niitä laajemmin kuten poistaa, muokata jne. App Service tarvitsee vain lukua runtimea varten, joten `Secrets User` on oikea minimi.

---

## Infrastructure as Code (Bicep)

**6.** Bicep-templatessa RBAC-roolimääritykset tehdään suoraan (`storageBlobRole`, `keyVaultSecretsRole`). Mitä etua tällä on verrattuna siihen, että ajat erilliset `az role assignment create` -komennot käsin?

> Vastauksesi:

Etu on toistettavuus ja vähemmän manuaalisia virheitä. Kun roolit on templaatissa, ne tulee aina samalla tavalla eikä jää vahingossa tekemättä kuten käsin ajettaessa joskus käy.

---

**7.** Bicep-parametritiedostossa `main.bicepparam` on `param moderationApiKey = ''` — arvo jätetään tyhjäksi. Miksi? Miten oikea arvo annetaan?

> Vastauksesi:

Koska oikea API-avain ei saa päätyä tiedostoon tai git-historiaan. tyhjä arvo toimii placeholderina.

Oikea arvo annetaan deploy-komennossa erikseen --parameters moderationApiKey=""

---

**8.** Bicep-templatessa `webApp`-resurssin `identity`-lohkossa on `type: 'SystemAssigned'`. Mitä tämä tekee, ja mitä manuaalista komentoa se korvaa?

> Vastauksesi:

Se laittaa Web Appille system-assigned managed identityn suoraan deployn aikana. Manuaalinen vastine olisi az webapp identity assign --name ... --resource-group ...

---

**9.** RBAC-roolimäärityksen nimi generoidaan `guid()`-funktiolla:

```bicep
name: guid(storageAccount.id, webApp.identity.principalId, 'StorageBlobDataContributor')
```

Miksi nimi generoidaan näin eikä esimerkiksi kovakoodatulla merkkijonolla? Mitä tapahtuisi jos nimi olisi sama kaikissa deploymenteissa?

> Vastauksesi:

`guid()` tekee nimestä deterministisen mutta uniikin kyseiselle scopelle ja resurssikombolle. Silloin sama deployment voidaan ajaa uudestaan ilman että syntyy satunnaisia ristiriitoja.

Jos nimi olisi kovakoodattu sama merkkijono kaikkialla, eri scopeissa tai eri ympäristöissä voisi tulla törmäyksiä ja deployment menisi herkemmin virheeseen

---

**10.** Olet nyt rakentanut saman infrastruktuurin kahdella tavalla: manuaalisesti (Osat 2 & 3) ja Bicepillä (Osa 3). Kuvaile konkreettisesti yksi tilanne, jossa IaC-lähestymistapa on selvästi manuaalista parempi. Kuvaile myös tilanne, jossa manuaalinen tapa riittää.

> Vastauksesi:

IaC on parempi kun tiimi pystyttää dev test prod ympäristöt toistuvasti. yhdellä komennolla saa saman rakenteen ja muutokset näkyy git diffissä.

Manuaalinen tapa riittää kun teet yhden pienen kokeilun kerran (esim testaat yhtä role assignmentia) etkä tarvitse siitä pysyvää toistettavaa mallia
