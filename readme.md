A projekt futtatható az alábbi módokon:

**Regisztrálás:**
dotnet run \<útvonal a csv-ket tartalmazó mappához> register \<profilUsername> \<profilPassword> \<email> \<firstname> \<lastname>
(mindegyiket meg kell adni, mert szeretem ha rendesen ki van töltve egy táblázat :D)

**Listázás:**
dotnet run \<útvonal a csv-ket tartalmazó mappához> list \<profilUsername> \<profilPassword>

**Profil törlése:**
dotnet run \<útvonal a csv-ket tartalmazó mappához> delete \<profilUsername> \<profilPassword>

**Jelszó hozzáadása egy profilhoz:**
dotnet run \<útvonal a csv-ket tartalmazó mappához> addpassword \<profilUsername> \<profilPassword> \<newVaultUsername> \<newVaultPassword> \<newWebsite>

**Jelszó törlése egy profilból a vaultban lévő username alapján:**
dotnet run \<útvonal a csv-ket tartalmazó mappához> deletepassword \<profilUsername> \<profilPassword> \<vaultUsername>

**Egy profilhoz tartozó összes jelszó törlése:**
dotnet run \<útvonal a csv-ket tartalmazó mappához> deletuserpasswords \<profilUsername> \<profilPassword>
