using Logic.Entity;
using Logic.MasterData;
using Logic.Types;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Character : SpriteObject
{
    private HpBar HpBar { get; set; }

    private Text StateText { get; set; }

    private GameObject TurnObject { get; set; }

    public double Hp { get; set; }

    public PlayerData PlayerData { get; set; }

    public PlayerDataClient PlayerDataClient { get; set; }

    public UnityGameController Controller { get; set; }

    private void Awake()
    {
        HpBar = GetComponentInChildren<HpBar>();
        StateText = transform.Find("State/Text").GetComponent<Text>();
        TurnObject = transform.Find("Turn").gameObject;

        OnAwake();
    }


    void Start()
    {
        SpriteSheetPath = PlayerDataClient.Sprite;

        Hp = PlayerData.Hp;

        OnStart();

        SetTexts();
        SetTurnObject(false);
    }

    public void SetTeam(TeamType teamType)
    {
        if (teamType == TeamType.BLUE)
        {
            HpBar.SetColor(new Color(0, 0, 255));
        }
        else
        {
            HpBar.SetColor(new Color(255, 0, 0));
        }
    }

    public void Attack()
    {
        ShowAttackRange();

        Animator.SetTrigger(PlayerDataClient.Attack);
    }

    public void Attacking()
    {
        if (string.IsNullOrEmpty(PlayerDataClient.ProjectTile))
        {
            Controller.OnAttacking();
        }
        else
        {
            Controller.OnThrowing();
        }
    }

    public IEnumerator Shot(Logic.Entity.PlayerPosition targetPosition)
    {
        var gameObj = Instantiate(Resources.Load("Prefabs/ProjectTile") as GameObject, transform.localPosition, Quaternion.identity, Controller.ProjectTilesTransform);
        var sprite = Resources.Load<Sprite>("ProjectTile/" + PlayerDataClient.ProjectTile);
        gameObj.GetComponentInChildren<SpriteRenderer>().sprite = sprite;

        var targetVector3 = targetPosition.PlayerPositionToVector3();
        while (0 < Vector3.Distance(gameObj.transform.localPosition, targetVector3))
        {
            Vector3 moveDirection = targetVector3 - gameObj.transform.localPosition;
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            gameObj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            gameObj.transform.localPosition = Vector3.MoveTowards(gameObj.transform.localPosition, targetVector3, 15f);
            yield return new WaitForSeconds(0.05f);
        }

        Destroy(gameObj);
        Controller.OnAttacking();
    }

    public void Damage()
    {
        Animator.SetTrigger("Hit");
    }

    public void DamageEnd()
    {
        Controller.TurnOffRanges();
        StartCoroutine(Controller.PopEvent());
    }

    public void HpChange(double hp)
    {
        Hp += hp;
        SetHpBar();
    }

    private void SetHpBar()
    {
        HpBar.SetValue((float)(Hp / PlayerData.Hp));
        SetTexts();
    }

    private void SetTexts()
    {
        HpBar.SetHpText(Hp, PlayerData.Hp);

        StateText.text = "";
    }


    public void SetTurnObject(bool active)
    {
        TurnObject.SetActive(active);

        if (active)
        {
            StartCoroutine(ShowMoveRange());
        }
    }

    private void ShowRange(int count, Color color)
    {
        for (int x = -count; x <= count; ++x)
        {
            for (int y = -count; y <= count; ++y)
            {
                if (Math.Abs(x) + Math.Abs(y) <= count)
                {
                    ShowRange(x, y, color);
                }
            }
        }
    }
    public IEnumerator ShowMoveRange()
    {
        Controller.TurnOffRanges();
        ShowRange(PlayerData.Move, new Color(0f, 0f, 0f, 0.4f));

        yield return new WaitForSeconds(1f);

        StartCoroutine(Controller.PopEvent());
    }

    public void ShowAttackRange()
    {
        Controller.TurnOffRanges();
        ShowRange(PlayerData.Range, new Color(255f, 0f, 0f, 0.4f));
    }

    private GameObject ShowRange(int x, int y, Color color)
    {
        var position = transform.localPosition.ToPlayerPosition();
        position.X += x;
        position.Y += y;

        var gameObj = Instantiate(Resources.Load("Prefabs/Range") as GameObject, position.PlayerPositionToVector3(), Quaternion.identity, Controller.RangesTransform);

        gameObj.GetComponent<SpriteRenderer>().color = color;

        return gameObj;

    }

    public void ShowStateText(string state)
    {
        StartCoroutine(ShowText(StateText, state, 1f));
    }

    public void ShowBuffText(string text)
    {
        // 버프 어케 표기하지?
    }

    public void ShowDamageText(double damage)
    {
        StartCoroutine(ShowText(StateText, damage.ToString(), 1f));
    }

    public void Dead()
    {
        Animator.SetTrigger("Dead");
        gameObject.transform.localPosition = gameObject.transform.localPosition.Change(0, 0, 50);
    }

    public void DeadEnd()
    {
        StartCoroutine(Controller.PopEvent());
    }

    public IEnumerator ShowText(Text text, string content, float time)
    {
        text.text = content;
        yield return new WaitForSeconds(time);
        text.text = "";
    }


    public IEnumerator Move(PlayerPosition targetPosition)
    {
        var targetVector3 = targetPosition.PlayerPositionToVector3();
        FlipX = targetVector3.x > transform.localPosition.x ? false : true;

        Animator.SetBool("Walking", true);
        while (0 < Vector3.Distance(transform.localPosition, targetVector3))
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetVector3, 5f);
            yield return new WaitForSeconds(0.05f);
        }
        Animator.SetBool("Walking", false);

        Controller.TurnOffRanges();
        StartCoroutine(Controller.PopEvent());
    }
}
