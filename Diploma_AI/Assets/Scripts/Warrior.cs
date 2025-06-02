using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Warrior : MonoBehaviour
{
    public float health;
    public float armor;
    public float damage;
    public float attackCooldown = 1f;
    public float moveSpeed = 1.5f;

    public SpriteRenderer _teamIdentifier;
    public SpriteRenderer _idleSprite;
    public SpriteRenderer _deadSprite;

    private float lastAttackTime;
    private List<Warrior> enemies = new List<Warrior>();
    private GameManager gameManager;
    private bool isAlive = true;
    private Warrior target;
    public int teamId;
    private int weaponLevel = 1;
    private Types type;

    public bool IsAlive => isAlive;

    public void Setup(List<Warrior> enemies, bool green, Types typed, GameManager manager)
    {
        this.enemies = enemies;
        this.gameManager = manager;
        type = typed;
        _idleSprite.gameObject.SetActive(true);
        _teamIdentifier.gameObject.SetActive(true);
        _deadSprite.gameObject.SetActive(false);
        _teamIdentifier.color = green ? Color.green : Color.red;
        teamId = green ? 1 : 0;
    }

    void Update()
    {
        if (!isAlive || enemies == null || enemies.Count == 0) return;

        target = SelectSmartTarget();
        switch (type)
        {
            case Types.Code:
                target = SelectSmartTarget();
                break;
            case Types.MLP:
                target = SelectStrategicTargetNM();
                break;
            case Types.Q:
                target = SelectNaiveTarget();
                break;
            case Types.RF:
                target = SelectCautiousTarget();
                break;
            default:
                target = SelectSmartTarget();
                break;
        }
        if (target == null || !target.IsAlive) return;

        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (health < 20 && distance < 1.5f)
        {
            Vector3 dir = (transform.position - target.transform.position).normalized;
            MoveSafely(dir);
            return;
        }

        if (distance <= gameManager.attackDistance)
        {
            if (Time.time - lastAttackTime > attackCooldown)
            {
                lastAttackTime = Time.time;
                target.TakeDamage(damage);
            }
        }
        else
        {
            Vector3 dir = (target.transform.position - transform.position).normalized;
            MoveSafely(dir);
        }
    }

    private void MoveSafely(Vector3 direction)
    {
        Vector3 nextPosition = transform.position + direction * moveSpeed * Time.deltaTime;
        nextPosition.x = Mathf.Clamp(nextPosition.x, gameManager.fieldMin, gameManager.fieldMax);
        nextPosition.z = Mathf.Clamp(nextPosition.z, gameManager.fieldMin, gameManager.fieldMax);
        transform.position = nextPosition;
    }

    public void TakeDamage(float amount)
    {
        if (_idleSprite != null)
        {
            StopAllCoroutines();
            StartCoroutine(FlashRed());
        }

        float effectiveHealth = health + armor;
        effectiveHealth -= amount;

        if (effectiveHealth <= 0)
        {
            Die();
        }
        else
        {
            if (armor >= amount)
                armor -= amount;
            else
            {
                float remaining = amount - armor;
                armor = 0;
                health = Mathf.Max(0, health - remaining);
            }
        }
    }

    private IEnumerator FlashRed()
    {
        _idleSprite.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        _idleSprite.color = Color.white;
    }

    private void Die()
    {
        isAlive = false;
        _idleSprite.gameObject.SetActive(false);
        _deadSprite.gameObject.SetActive(true);
        _teamIdentifier.gameObject.SetActive(false);
        gameManager.CheckVictoryCondition();
    }

    private Warrior FindClosestEnemy()
    {
        float minDist = float.MaxValue;
        Warrior closest = null;
        foreach (var enemy in enemies)
        {
            if (enemy == null || !enemy.IsAlive) continue;
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }

    private Warrior SelectSmartTarget()
    {
        Warrior bestTarget = null;
        float bestScore = float.MinValue;

        foreach (var enemy in enemies)
        {
            if (enemy == null || !enemy.IsAlive) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            float healthScore = 100 - (enemy.health + enemy.armor); // менше здоровТ€ Ч краще
            float distanceScore = 5 - dist;                         // ближче Ч краще
            float score = healthScore + distanceScore;

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = enemy;
            }
        }
        return bestTarget;
    }

    private Warrior SelectStrategicTargetNM()
    {
        Warrior bestTarget = null;
        float bestScore = float.MinValue;

        foreach (var enemy in enemies)
        {
            if (enemy == null || !enemy.IsAlive) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            float threatLevel = (enemy.damage + enemy.weaponLevel * 2f) - (enemy.health + enemy.armor * 0.5f); // високий р≥вень загрози Ч менше бажанн€ атакувати

            float healthScore = 100 - (enemy.health + enemy.armor); // менше Ч краще
            float distanceScore = 5 - dist;
            float score = healthScore + distanceScore - threatLevel;

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = enemy;
            }
        }

        return bestTarget;
    }

    private Warrior SelectNaiveTarget()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.IsAlive)
                return enemy;
        }
        return null;
    }

    private Warrior SelectCautiousTarget()
    {
        int nearbyEnemies = 0;
        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.IsAlive)
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < 2.5f) nearbyEnemies++;
            }
        }

        // якщо ворог≥в поруч б≥льше 2 або здоров'€ дуже низьке Ч не атакуЇ
        if (nearbyEnemies > 2 || health < 30)
            return null;

        return FindClosestEnemy(); // fallback
    }

    private void LogAction(int actionCode, string result)
    {
        float distanceToEnemy = target != null ? Vector3.Distance(transform.position, target.transform.position) : 0f;
        BattleLogger.Instance.LogAgentData(
            teamId: teamId,
            position: transform.position,
            health: health,
            armor: armor,
            weaponLevel: weaponLevel,
            distanceToEnemy: distanceToEnemy,
            action: actionCode,
            result: result,
            isAlive: isAlive
        );
    }


}

public enum Types
{
    Code,
    MLP,
    Q,
    RF
}
