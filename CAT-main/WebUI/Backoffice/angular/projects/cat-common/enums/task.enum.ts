export enum Task {
  Unknown = -1,
  Jobboard = 1,
  AIProcess = 2,
  Translation = 3,
  Revision = 4,
  ClientReview = 5,
  Completed = 6,
  CreditLinguists = 7,
  Billing = 8,
  Delivery = 9,
  End = 100
}

export const TaskDisplayName: { [key : number]: string } = {
  [Task.Translation]: "Translation",
  [Task.Revision]: "Revision",
  [Task.Unknown]: "Unknown task",
  [Task.Jobboard]: "Job board",
  [Task.AIProcess]: "AI process",
  [Task.ClientReview]: "Client review",
  [Task.Completed]: "Job completed",
  [Task.CreditLinguists]: "Credit",
  [Task.Billing]: "Billing",
  [Task.Delivery]: "Delivery",
  [Task.End]: "End"
};
