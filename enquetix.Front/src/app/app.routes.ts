import { Routes } from "@angular/router";

export const routes: Routes = [
  {
    path: "",
    loadComponent: () =>
      import("./pages/index/index.component").then((m) => m.IndexComponent),
  },
  {
    path: "create-edit-poll",
    loadComponent: () =>
      import("./pages/create-edit-poll/create-edit-poll.component").then(
        (m) => m.CreatePollComponent
      ),
  },
  {
    path: "poll",
    loadComponent: () =>
      import("./pages/poll/poll.component").then((m) => m.PollComponent),
  },
];
