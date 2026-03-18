# Documentation — Sécurité de la base de données

## Choix architectural : accès direct aux tables

### Approche retenue

L'application utilise **l'accès direct aux tables** via Entity Framework Core, avec des utilisateurs SQL dédiés disposant de droits granulaires.

### Utilisateurs SQL

| Utilisateur | Droits | Utilisation |
|---|---|---|
| `padel_app_user` | `SELECT`, `INSERT`, `UPDATE`, `DELETE` sur toutes les tables | Connection string de l'API (opérations CRUD) |
| `padel_readonly` | `SELECT` uniquement sur toutes les tables | Reporting, statistiques, accès en lecture seule |

### Justification du choix

| Critère | Accès direct aux tables | Procédures stockées uniquement |
|---|---|---|
| **Simplicité** | ✅ Simple, compatible EF Core nativement | ❌ Nécessite d'écrire et maintenir chaque procédure |
| **Maintenabilité** | ✅ Logique métier centralisée dans les services C# | ❌ Duplication de la logique entre C# et SQL |
| **Compatibilité ORM** | ✅ EF Core fonctionne directement avec les tables | ❌ Nécessite `FromSqlRaw` ou mappings spéciaux |
| **Évolutivité** | ✅ Ajout de colonnes/tables sans modifier de procédures | ❌ Chaque changement de schéma impacte les procédures |
| **Sécurité** | ⚠️ L'utilisateur peut exécuter des requêtes arbitraires sur les tables autorisées | ✅ Contrôle total : seules les opérations prévues sont possibles |
| **Performance** | ⚠️ Requêtes générées par l'ORM (optimisables) | ✅ Requêtes SQL optimisées manuellement |

### Mesures de sécurité compensatoires

1. **Droits granulaires** : chaque GRANT est appliqué table par table, pas de `db_datareader` / `db_datawriter` global
2. **Utilisateur séparé pour le reporting** : `padel_readonly` ne peut que lire, empêchant toute modification accidentelle
3. **Pas d'accès `dbo` ni `sa`** : l'application n'utilise jamais un compte administrateur
4. **Validation côté applicatif** : toutes les règles métier sont validées dans les services C# avant d'atteindre la base
5. **Entity Framework Core** : les requêtes paramétrées protègent contre l'injection SQL

### Conclusion

L'accès direct aux tables est le choix le plus adapté pour ce projet car :
- Il est **nativement compatible** avec Entity Framework Core
- Il **évite la duplication** de la logique métier
- Les **droits granulaires** par utilisateur SQL compensent le risque de requêtes non prévues
- La **simplicité** de maintenance est critique pour un projet de cette taille
