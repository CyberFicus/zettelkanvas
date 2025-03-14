# Общие сведения
Zettelkanvas - это инструмент, позволяющий адаптировать холсты в Obsidian.md для ведения записок в методике зеттелькастен. Zettelkanvas поддерживает работу с YAML-метаданными заметок в Obsidian.md, позволяет настраивать вид отображения заметок на холст через конфигурационный файл, а также парсит ссылки между заметками и дописывает их в конце заметки с псевдонимами (если такая опция включеня в конфигурации), также может генерировать собственные ссылки в соответствии с иерархией заметок.
Подключение к Obsidian.md осуществляется через размещение .exe файла zettelkanvas и файла с конфигурацией в Obsidian-хранилище, после чего через плагин Shell Commands для Obsidian.md следует настроить вызов exe-файла по нажатию горячей клавиши. 
Исполняемый файл принимает три аргумента: расположение сканируемой директории относительно корня хрналища, расположение и название (без указания формата!) результирующего файла холста относительно корня хранилища и расположение файла конфигурации относительно хранилища.

В этом репозитории в директории ZKTest расположено хранилище Obsidian.md, с которым программа работает в режиме отладки. Оно также может пригодиться для ознакомления с работой zettelkanvas.
Зеттелькастен-заметки должны иметь названия в нотации Лумана (см. З. Аренс "Как делать полезные заметки"): название начинается с последовательности цифр и латинских букв, указывающих расположение и связи относительно других заметок. В дальнейшем эта последовательность называется идентификатором файла/узла.
Допустимо после такой последовательности через пробел указывать название заметки.

В настоящее время проект заброшен, я даже не писать README на английском, как изначально собирался. 

Известные баги:
- Ошибка работы, если в сканируемой директории отсутствуют заметки с названиями верного формата
# Более подробные сведения и терминология
Заметки расставляются в соответствии со структурой, определяемой их названиями. У каждой заметки может быть некоторое число потомков, из которых первый будем называть наследником, а остальные - ветвями. Заметки вместе с потомками организуются в "деревья".

Чтобы сделать заметку основанием нового дерева, следует добавить ей YAML-свойство root и установить ему значение true или 1. Такие узлы в дальнейшем будем называть корневыми.

Есть два режима расстановки заметок в дереве: вширь (слева) и вглубь (справа), отличающиеся положением наследника. В первом режиме наследник не сдвигается вправо, во втором - сдвигается

![image](https://github.com/user-attachments/assets/784b209a-7d24-4828-a9e6-58aea10e1bf1)

Если разрешена инверсия расстановки, то к потомкам заметки с YAML-свойством inverse_arrangement установленным в true или 1 будет применён альтернативный режим расстановки ("вглубь", если в норме к заметкам применяется "вширь", и наоборот)

Ссылки между заметками на холсте отображаются в виде линий с (опционально) стрелками. Ссылки бывают двух видов: древообразующие (оранжевого цвета на рисунке) и внешние (синего цвета на рисунке). Внешними являются все ссылки, относящиеся не к предку/потомку заметки _(важно: предки/потомки не в первом поколении тоже считаются внешними ссылками)_ 

Также есть режим "сжатия" деревьев, работающий как при расстановке вширь (слева), так и вглубь (справа)

![image](https://github.com/user-attachments/assets/be51fef2-9ba9-4e70-b985-5bdb07b10571)

После текста каждой заметки zettelkanvas добавляет строку %%ZK:links%% _(не отображается при отображении заметок в Obsidian.md в режиме просмотра)_, обозначающую начало раздела ссылок
На рисунке приведён вид на холсте и в редакторе в режиме отображения исходного кода

![image](https://github.com/user-attachments/assets/e19bfd84-953d-4445-98ca-cc756c764de1)

В разделе ссылок ссылка на родительскую заметку сопровождается знаком "<", ссылка на наследника - знаком ">", ссылки на ветви - знаком ⊢ (он же $vdash$), а внешние ссылки из текузей заметки - знаком ⊙ (он же $odot$). Если у ссылки есть отображаемый текст _(например [[ИмяЗаметки|отображаемый текст]])_, то в разделе ссылок напротив имени-ссылки будет отображаться именно он (см. ссылку на 2ab в примере). Если ссылка на одну и ту же заметку дублируется, будет приведен отображаемый текст первой из них (даже если там отображаемого текста нет, см. ссылка на 4b в примере). Если включён режим отображения псевдонимов, то отображаемый текст в разделе ссылок будет заменен на первое значение YAML-свойства aliases той заметки, на которую делается ссылка (см. ссылку 2a3 в примере, тут вместо отображаемого текста Vivamus rutrum показывается строка "an alias!", записанная в свойстве aliases у соответствующей заметки). Также, если у заметки помимо идентифкатора есть ещё и некоторое осмысленное имя, оно будет скрываться в разделе заметок _(опять же, см. 2a3)_

# Настройки и конфигурационный файл
Файл позволяет задавать:
- Размеры заметок на холсте (note_width, node_height) - число, устанавливаются числом, не рекомендуется ставить меньше 300
- Расстояние между заметками на холстве (node_distance) - число, не рекомендуется ставить меньше 150
- Режим расстановки заметок (use_long_arrangement, allow_inverse_arrangement) - 0 или 1 
- Режим "ужатия" деревьев (shrink_trees) - 0 или 1
- Автоматическое указание псевдонимов ссылок (link_auto_alias) - подстановка первого значения YAML-свойства aliases заметки, на которую указана ссылка, 0 или 1
- Расположение стрелок на связах между заметками (default_edge_arrow, default_tree_link_arrow, default_outer_link_arrow) - число от 0 до 3 или прочерк. default_edge_arrow применяется ко всем создаваемым связям на холсте, следующие два параметра - определяют внешний вид древообразующих и внешних ссылок соответственно, и при определении перекрывают значение default_edge_arrow
- Окрас элементов холста (@default_edge_color, @default_tree_link_color , @default_outer_link_color, @default_node_color) - знак минус, если цвет не установлен, цифра от 1 до 6 для одного из стандартных цветов по умолчанию, или цвет в hex-записи. 

После каждого параметра и перед определенеим значения должно стоять двоеточие, желательно отделённое пробелами с обеих сторон.

Пример файла конфигурации из ZKTest:
```
// Delete this file to reset config
// Canvas parameters:
node_width : 400
node_height : 500
node_distance : 150
use_long_arrangement : 1
allow_inverse_arrangement : 1
shrink_trees : 1
link_auto_alias : 1
// Arrows: 0 - none, 1 - forward, 2 - reverse, 3 - two-sided 
default_edge_arrow : 1
default_tree_link_arrow : 0
default_outer_link_arrow : 3
// Color constants: 
@default_edge_color : -
@default_tree_link_color : 2
@default_outer_link_color : #0b2ca1
@default_node_color : 4
```
# Подробнее про принцип работы
В общем виде после запуска программы она считывает файл конфуигарции и устаналивает соответствующие параметры работы, затем парсит содержимое указанной при запуске папки, обрабатывает его и записывает результат обработки в json-файл с форматом .canvas, который Obsidian.md сможет отрендерить как холст.

Обработка делится не следующие шаги:
1. Получение списка файлов, будущих узлов дерева, из указанной директории - функция GetNodesFromDir. Все файлы, имеющие формат, отличный от .md или не начинающиеся с верно отформатированного идентификатора, отбрасываются. При повторении идентификатора файл также отбрасывается. В процессе для каждого файла создаётся сущность Node, для чего в т.ч. парсятся его YAML-свойства.
2. Построение деревьев - функция BuildTrees. Включает в себя следующие шаги:
   1. Сортировка списка узлов по их идентификаторам
   2. Установление связей между узлами, которые в дальнейшем будут преобразованы в древообразующие ссылки, если такие ссылки не проставлены в тексте самой заметки. Координаты узлов пока не устанавливаются
3. Расстановка узлов деревеьв - функция ArrangeNodes. Именно на этом этапе выставляются координаты узлов. Для каждого из корневых узлов выполняется следующие шаги:
   1. Вертикальный сдвиг относительно предыдущего дерева, чтобы не было наложения деревьев друг на друга
   2. Вызов рекурсивного алгоритма расстановки потомков.
4. Сжатие деревьев, если включено в настройках - функция ShrinkTrees
   1. Определение габаритов холста
   2. Создание бинарной карты (двумерный массив bool), отображающей текущее расположение всех узлов всех деревьев
   3. Рекурсивный вызов функции сжатия Node.Shrink для каждого из коренвых узлов
5. Создание списка связей на холсте - функция BuildEdges. Для каждого файла выполняется
   1. Вызов фукнции парсинга ссылок Node.ProcessNote, возвращаюшей список связей-ссылок
   2. Добавление полученного множества связей к общему списку
6. Преобразование списков узлов и связей в json-файл холста
# Некоторые детали
## Рекурсивная расстановка узлов дерева
Каждый узел расставляет свои дочерние узлы и после этого считает, сколько места он вместе с расставленными узлами занимает (высота и горизонтальная длина прямоугольника области, занимаемой узлом и его дочерними узлами), и возвращает это значение родительскому узлу, чтоб тот знал как расставлять остальные дочерние узлы. 

На рисунке в порядке от зелёного к красному показаны вложенные прямоугольники занимаех областей дочерних узлов, в ромбе - расставляющий в соответствующей итерации узел:

![image](https://github.com/user-attachments/assets/95622691-be69-407e-bc0e-dc731481d0ec)

В режиме расстановки "вширь" последовательность действий выглядит так: 
1. Поставь справа от себя наследника
2. Наследник расставляет свои дочерние узлы, возвращает длину и высоту своей занимаемой области
3. Сделай сдвиг с учётом полученной высоты, поставь туда первую из ещё не проставленных ветвей
4. Эта ветвь расставляет свои дочерние узлы, возвращает длину и ширину высоту своего прямоугольника
5. Остались нерасставленные ветви? Вернись на шаг 3
6. Верни высоту и ширину своего прямоугольника родительскому узлу

А вот те же узлы, но расстанове вглубь. Тут всё в целом так же, но сначала расставляются ветви, а затем - наследник с выносом вправо. Причём вынос не на всю длину - а на длину-1, потому что непосредственно под наследником точно не будет его дочерних узлов, т.е. столкновения не будет, а место экономится.

![image](https://github.com/user-attachments/assets/e2ea9d72-8dfb-4ffc-8b21-94ffc52dd555)


```CSharp
public void Arrange(out int length, out int height) // универсальная расстановка, результат определяется режимом расстановки конкретного узла
{
   if (Parameters.UseLongArrange ^ (this.InverseArrangement && Parameters.AllowInverseArrangement))
       ArrangeLong(out length, out height);
   else 
       ArrangeWide(out length, out height);
}

private void ArrangeWide(out int length, out int height) // расстановка "вширь"
{
   length = 1;
   height = 1;
   int lengthBuf = 0, 
       heightBuf = 0;
   
   if (Next is not null)
   {
       Next.MoveFromNode(this, 1, 0); // сдвиг на один "слот" для узла вправо, фактически учитывает размер узла и расстояние между ними из конфига
       Next.Arrange(out lengthBuf, out heightBuf);
       length += lengthBuf;
       height = int.Max(height, heightBuf);
   }

   for (int i = 0; i < Branches.Count; i++)
   {
       Branches[i].MoveFromNode(this, 1, height); // сдвиг на единицу вправо и на текущую высоту занимаемой области 
       Branches[i].Arrange(out lengthBuf, out heightBuf);
       height += heightBuf;
       length = int.Max(length, lengthBuf+1);
   }
}

private void ArrangeLong(out int length, out int height) // расстановка "вглубь"
{
   length = 1; 
   height = 1;
   int lengthBuf = 0,
       heightBuf = 0;
   for (int i = 0; i < Branches.Count; i++)
   {
       Branches[i].MoveFromNode(this, 1, height); 
       Branches[i].Arrange(out lengthBuf, out heightBuf);
       height += heightBuf;
       length = int.Max(length, lengthBuf);
   }

   if (Next is not null)
   {
       Next.MoveFromNode(this, length, 0);
       Next.Arrange(out lengthBuf, out heightBuf);
       length += lengthBuf; 
       height = int.Max(height, heightBuf);
   }
}
```
## Сжатие деревьев
Чтобы сжать дочерние узлы некоторого узла, следует сделать следующие шаги:
1. Сжать наследника (да, снова рекурсия!)
2. Сжать все ветви
3. Попытаться сдвинуть каждую из ветвей вверх, пока не будет столкновения
4. Сдвигать сам родительский узел вверх, пока не будет столкновения или не будет достигнут предел допустимых координат (дочерний узел не может быть выше родительского, корневой узел не может быть выше или левее координаты 0,0)

Код ShrinkTrees
```CSharp
private static void ShrinkTrees(List<Node> nodes, List<Node> rootNodes, Dictionary<string, Node> idToNode)
{
   // Определение габаритов холста
   int maxX = 0, maxY = 0;
   foreach (Node node in nodes)
   {
       if (node.X > maxX) maxX = node.X;
       if (node.Y > maxY) maxY = node.Y;
   }
   // Создание карты холста по габаритам, заполнение: где есть узел, там 1, иначе 0
   bool[,] map = new bool[maxX + 1, maxY + 1];
   foreach (Node node in nodes)
       map[node.X, node.Y] = true;
   // Рекурсивное сжатие дочерних узлов каждого из корневых узлов 
   foreach (Node root in rootNodes)
       root.Shrink(map);
}
```
Далее "карта" узлов map активно используется для определения, есть ли в конкретном месте другой узел (и, следовательно, можно ли в это место сдвинуть текущий узел и его дочерние узлы): 
Сама функция Node.Shrink выглядит так:
```CSharp
public void Shrink(bool[,] map)
{
    // Рекурсивные вызовы Node.Shrink
    if (Next is not null)
        Next.Shrink(map);
    foreach (var branch in Branches)
        branch.Shrink(map);

    int GetMinYForRoot() // Определеяет минимальную допустимую координату Y для корневого узла
    {
        bool LineIsFree(int y) // возвращает true, если текущий корневой узел (и всё его дерево) можно сдвинуть на единицу вверх
        {
            if (y < 0) return false;
            for (int x = 0; x < map.GetLength(0); x++)
            {
                if (map[x, y])
                    return false;
            }
            return true;
        }
        int minY = Y-1;
        while (LineIsFree(minY) && minY >=0)
            minY--; // Сдвигает минимальную возможную координату Y для корневого узла, определяя до какого момента будут осуществляться попытки подъёма узла

        return minY+1;
    }

    int minY = (Parent is null) ? GetMinYForRoot() : Parent.Y+1; 

    bool res = true;
    while (Y > minY && res)
        res = TryShiftUp(map); // "Если можешь сдвинуть этот узел - сдвинь его. Если получишь false - прекрати пытаться"
}
```

### Визуальная демонстрация алгоритма
Взят пример расстановки узлов вширь. Текущая активная область выделяется фиолетовым цветом.

Сжимаем зелёный. Сначала надо сжать наследника

![image](https://github.com/user-attachments/assets/cc04a926-7860-4f77-97d5-84cf43d3e9d8)

Сжимаем наследника

![image](https://github.com/user-attachments/assets/21e26b21-5b7f-4545-92fc-2b679d990b6d)

Сжимаем дочерние узлы наследника. У них и так всё хорошо, ничего не меняется

![image](https://github.com/user-attachments/assets/79229b34-1030-4092-b746-bdcfc934b241)

Пробуем сдвинуть первую ветвь. Некуда (ветвь не может находиться на одном уровне с родителем, только наследник). Тут выделение ветви - синим

![image](https://github.com/user-attachments/assets/1ccb767f-5d2d-46db-9610-a7ff7ee4b1c1)

Смотрим, можно ли сдвинуть вторую ветвь. Можно. Сдвинем до предела (1 шаг, результат сдвига см. через картинку) 

![image](https://github.com/user-attachments/assets/ce388413-c7b8-4289-ae99-b23e3113f5bd)

Смотрим, можно ли сдвинуть третью ветвь. Можно. Сдвинем до предела (3 шага, результат сдвига см. через картинку)

![image](https://github.com/user-attachments/assets/bff04ba7-c224-40b4-9286-ebe9a2920e6d)

Это программно никак не учитывается, но фактически размер дерева уменьшился на три клетки по вертикали:

![image](https://github.com/user-attachments/assets/b2508ff8-833c-4016-b28e-fc063237e5aa)

С наследником зелёного узла закончили, переходим к его ветвям

![image](https://github.com/user-attachments/assets/827fdbe1-ea69-4d64-a040-ef76cdd77f93)

Сжимаем первую ветвь. Здесь и так всё хорошо, от сжатия ничего не изменится. Переходим дальше

![image](https://github.com/user-attachments/assets/600ea8bb-e866-4892-9891-1e9ea1da8139)

Сжимаем третью ветвь, тут и так всё хорошо, от сжатия ничего не изменится. Сжимать дочерние узлы закончили, переходим к сдвигам.
Смотрим, можно ли сдвинуть первую ветвь. Можно. Сдвигаем до предела (3 шага)
![image](https://github.com/user-attachments/assets/8e9714b5-7823-4687-8c44-d0971533ae33)

Смотрим, можно ли сдвинуть вторую ветвь. Можно. Сдвигаем до предела (5 шагов)

![image](https://github.com/user-attachments/assets/5a630755-6e08-4fca-a0d1-75e602bcfb39)

Это, опять же, программно нигде не учитывается, но фактически размеры прямоугольника сократились на 4 в высоту.
Было (слева) и стало (справа)

![image](https://github.com/user-attachments/assets/3b668ed3-bf42-47d5-acda-47e18a7dc40a)
