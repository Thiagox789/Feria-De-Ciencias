using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LocalLeaderboardController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private int recordsLimit = 20;

    private Button refreshButton;
    private Button submitButton;
    private Button deleteButton;
    private Button previousButton;
    private Button nextButton;
    private LongField scoreField;
    private ListView recordsList;
    private ScrollView scrollView;

    private VisualElement errorPopup;
    private Button errorCloseButton;
    private Label errorMessage;

    private VisualElement ownerRecordElement;

    private int currentPage = 0;
    private List<RecordsData.EntradaRanking> allRecords = new List<RecordsData.EntradaRanking>();

    private void Start()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        InitializeUI();
        LoadRecords();
    }

    private void InitializeUI()
    {
        var root = uiDocument.rootVisualElement;

        refreshButton = root.Q<Button>("refresh");
        submitButton = root.Q<Button>("leaderboard-submit");
        deleteButton = root.Q<Button>("leaderboard-delete");
        previousButton = root.Q<Button>("previous-page");
        nextButton = root.Q<Button>("next-page");

        scoreField = root.Q<LongField>("score-field");
        recordsList = root.Q<ListView>("records-list");
        scrollView = recordsList.Q<ScrollView>();

        errorPopup = root.Q<VisualElement>("error-popup");
        errorMessage = root.Q<Label>("error-message");
        errorCloseButton = root.Q<Button>("error-close");

        ownerRecordElement = root.Q<VisualElement>("owner-record");

        if (refreshButton != null)
            refreshButton.clicked += LoadRecords;

        if (submitButton != null)
            submitButton.clicked += SubmitScore;

        if (deleteButton != null)
            deleteButton.clicked += DeleteRecord;

        if (previousButton != null)
            previousButton.clicked += PreviousPage;

        if (nextButton != null)
            nextButton.clicked += NextPage;

        if (errorCloseButton != null)
            errorCloseButton.clicked += () => errorPopup.style.display = DisplayStyle.None;

        recordsList.makeItem = () => CreateRecordView();
        recordsList.bindItem = (element, index) => BindRecordView(element, index);
    }

    public void LoadRecords()
    {
        if (DataManager.Instancia == null)
        {
            ShowError("DataManager no encontrado");
            return;
        }

        allRecords = DataManager.Instancia.ObtenerRanking();
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        int startIndex = currentPage * recordsLimit;
        int endIndex = Mathf.Min(startIndex + recordsLimit, allRecords.Count);
        int totalRecords = allRecords.Count;

        recordsList.Clear();

        for (int i = startIndex; i < endIndex; i++)
        {
            var record = allRecords[i];
            var element = CreateRecordView();
            BindRecordViewData(element, record, i + 1);
            recordsList.Add(element);
        }

        previousButton.SetEnabled(currentPage > 0);
        nextButton.SetEnabled(endIndex < totalRecords);

        if (scrollView != null)
            scrollView.scrollOffset = Vector2.zero;

        UpdateOwnerRecord();
    }

    private VisualElement CreateRecordView()
    {
        var element = new VisualElement();
        element.style.flexDirection = FlexDirection.Row;
        element.style.height = 75;

        var rankLabel = new Label();
        rankLabel.name = "rank";
        rankLabel.style.width = new Length(10, LengthUnit.Percent);
        rankLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        element.Add(rankLabel);

        var usernameLabel = new Label();
        usernameLabel.name = "username";
        usernameLabel.style.width = new Length(60, LengthUnit.Percent);
        usernameLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
        element.Add(usernameLabel);

        var scoreLabel = new Label();
        scoreLabel.name = "score";
        scoreLabel.style.width = new Length(14, LengthUnit.Percent);
        element.Add(scoreLabel);

        var gap = new VisualElement();
        gap.style.width = new Length(2, LengthUnit.Percent);
        element.Add(gap);

        var subScoreLabel = new Label();
        subScoreLabel.name = "sub-score";
        subScoreLabel.style.width = new Length(14, LengthUnit.Percent);
        element.Add(subScoreLabel);

        return element;
    }

    private void BindRecordView(VisualElement element, int index)
    {
        int actualIndex = currentPage * recordsLimit + index;
        if (actualIndex < allRecords.Count)
        {
            var record = allRecords[actualIndex];
            BindRecordViewData(element, record, actualIndex + 1);
        }
    }

    private void BindRecordViewData(VisualElement element, RecordsData.EntradaRanking record, int rank)
    {
        var rankLabel = element.Q<Label>("rank");
        var usernameLabel = element.Q<Label>("username");
        var scoreLabel = element.Q<Label>("score");
        var subScoreLabel = element.Q<Label>("sub-score");

        if (rankLabel != null)
        {
            if (rank <= 3)
            {
                Color medalColor = rank == 1 ? Color.yellow :
                                   rank == 2 ? Color.gray :
                                   new Color(0.8f, 0.5f, 0.2f);
                rankLabel.style.color = medalColor;
                rankLabel.text = rank.ToString();
            }
            else
            {
                rankLabel.text = rank.ToString();
                rankLabel.style.color = Color.white;
            }
        }

        if (usernameLabel != null)
            usernameLabel.text = record.nombreJugador;

        if (scoreLabel != null)
            scoreLabel.text = FormatearTiempo(record.tiempo);

        if (subScoreLabel != null)
            subScoreLabel.text = record.fecha;
    }

    private void UpdateOwnerRecord()
    {
        if (ownerRecordElement == null) return;

        var rankLabel = ownerRecordElement.Q<Label>("rank");
        var usernameLabel = ownerRecordElement.Q<Label>("username");
        var scoreLabel = ownerRecordElement.Q<Label>("score");
        var subScoreLabel = ownerRecordElement.Q<Label>("sub-score");

        if (rankLabel != null)
            rankLabel.text = "";

        if (usernameLabel != null)
            usernameLabel.text = "Tu tiempo";

        if (scoreLabel != null)
            scoreLabel.text = "--";

        if (subScoreLabel != null)
            subScoreLabel.text = "--";
    }

    private void SubmitScore()
    {
        if (DataManager.Instancia == null) return;

        float score = scoreField != null ? scoreField.value : 0;
        if (score <= 0)
        {
            ShowError("Ingresa un tiempo valido");
            return;
        }

        DataManager.Instancia.AgregarAlRanking("Jugador", score);
        LoadRecords();
    }

    private void DeleteRecord()
    {
        if (DataManager.Instancia == null) return;

        DataManager.Instancia.LimpiarRanking();
        LoadRecords();
    }

    private void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateDisplay();
        }
    }

    private void NextPage()
    {
        int maxPages = Mathf.CeilToInt((float)allRecords.Count / recordsLimit);
        if (currentPage < maxPages - 1)
        {
            currentPage++;
            UpdateDisplay();
        }
    }

    private void ShowError(string message)
    {
        if (errorPopup != null)
            errorPopup.style.display = DisplayStyle.Flex;

        if (errorMessage != null)
            errorMessage.text = message;
    }

    private string FormatearTiempo(float tiempo)
    {
        int min = (int)(tiempo / 60f);
        int seg = (int)(tiempo % 60f);
        int mili = (int)((tiempo - Mathf.Floor(tiempo)) * 1000f);
        return string.Format("{0:00}:{1:00}.{2:000}", min, seg, mili);
    }
}
