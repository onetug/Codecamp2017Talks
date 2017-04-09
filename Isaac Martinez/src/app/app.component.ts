import { Component } from '@angular/core';
import { TodoService } from './todo.service';
import { OnInit } from '@angular/core';
import { HttpModule } from '@angular/http';
import { TodoItem } from './todo-item';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {

  todos: TodoItem[];
  newTodo: TodoItem;
  addClicked: boolean;

  constructor(private todoSvc: TodoService) {
    this.newTodo = new TodoItem();
  }

  ngOnInit(): void {
    this.addClicked = false;
    this.todoSvc.getTodos()
      .then(t => this.todos = t);
  }

  saveChanges(): void {
    this.addClicked = false;
    this.todos.push(this.newTodo);
    this.todoSvc.saveTodo(this.newTodo);
    this.newTodo = new TodoItem();
  }

  delete(todo: TodoItem, index: number): void {
    this.todoSvc.deleteTodo(todo.id);
    this.todos.splice(index, 1);
  }
}
