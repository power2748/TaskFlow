using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.Tasks
{
    public enum TaskStatus
    {
        ToDo = 0,         // Нужно сделать
        InProgress = 1,   // В процессе
        CodeReview = 2,   // На проверке
        Done = 3          // Готово
    }
}
