using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DotNetMicroBenchmarks.Benchmarks;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;

namespace DotNetMicrobenchmarks.Unity.Main
{
    public class MainBehaviour : MonoBehaviour
    {
        public UIDocument UiDocument;
        private GridView _grid;
        private TextArea _textArea;

        // private readonly object _lock = new();
        private readonly List<string> _messages = new();

        private void Start()
        {
            var root = UiDocument.rootVisualElement;
            root.Q<Button>("action-button")
                .clickable.clicked += StartBenchmark;

            _grid = root.Q<GridView>("grid");
            _grid.itemHeight = 60;
            _grid.columnCount = 1;
            _grid.makeItem = () =>
            {
                var text = new Text();
                var itemRoot = new VisualElement();
                itemRoot.Add(text);
                return itemRoot;
            };
            _grid.bindItem = (element, index) =>
            {
                var text = element.Q<Text>();
                text.size = TextSize.XL; 
                var type = (Type) _grid.itemsSource[index];
                text.text = $"{type!.Name}";
            };
            _grid.itemsSource = BenchmarkRunner.GetAllBenchmarkSuites(Assembly.GetExecutingAssembly()).ToList();
            _grid.selectionType = SelectionType.Multiple;
            _grid.selectionChanged += selection => Debug.Log($"Selection changed: {string.Join(", ", selection)}");
            _grid.itemsChosen += selection => Debug.Log($"Items chosen: {string.Join(", ", selection)}");
            _grid.doubleClicked += indexUnderMouse => Debug.Log($"Double clicked: {indexUnderMouse}");
            _textArea = root.Q<TextArea>("console");
        }

        private void Update()
        {
            lock (_messages)
            {
                if (_messages.Count > 0)
                {
                    _textArea.value += "\n" + string.Join("\n", _messages);
                    
                    _messages.Clear();
                }
            }
        }

        private void StartBenchmark()
        {
            var selected = _grid.selectedIndex;
            if (selected < 0)
            {
                Debug.Log("No benchmarks selected");
                return;
            }

            var benchmarkSuite = (Type) _grid.itemsSource[selected];
            Debug.Log($"{benchmarkSuite.Name} selected");

            var job = new Thread(() =>
            {
                AppendLog($"{benchmarkSuite.Name} running");
                BenchmarkRunner.Run(benchmarkSuite, 3, AppendLog);
            });
            
            job.Start();
        }

        private void AppendLog(string log)
        {
            lock (_messages)
            {
                Debug.Log($"Received Message: {log}");
                _messages.Add(log);
            }
        }
    }
}