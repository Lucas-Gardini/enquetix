import { Routes } from "@angular/router";

export const routes: Routes = [
  {
    path: "",
    loadComponent: () =>
      import("./pages/index/index.component").then((m) => m.IndexComponent),
  },
  {
    path: "create-poll",
    loadComponent: () =>
      import("./pages/create-poll/create-poll.component").then(
        (m) => m.CreatePollComponent
      ),
  },
];
