namespace LearnApi.Models;

public class TodoItemDto
{
     public long Id { get; set; }
     public string? Name { get; set; }
     public bool IsComplete { get; set; }   
 
     public TodoItemDto(){ }
     public TodoItemDto(TodoItem todoItem)
     {
         Id = todoItem.Id;
         IsComplete = todoItem.IsComplete;
         Name = todoItem.Name;
     }    
}