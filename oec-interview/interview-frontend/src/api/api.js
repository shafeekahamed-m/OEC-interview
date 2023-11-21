import {
  AddUserToProcedureError,
  GetPlanProcedures,
  GetProceduresError,
  GetUserAssignmentsError,
  GetUsersError,
  StartPlanError,
} from "../constants/ApiConstants";

const api_url = "http://localhost:10010";

const postHeaders = {
  Accept: "application/json",
  "Content-Type": "application/json",
};

export const startPlan = async () => {
  const url = `${api_url}/Plan`;
  const response = await fetch(url, {
    method: "POST",
    headers: postHeaders,
    body: JSON.stringify({}),
  });

  if (!response.ok) throw new Error(StartPlanError);

  return await response.json();
};

export const addProcedureToPlan = async (planId, procedureId) => {
  const url = `${api_url}/Plan/AddProcedureToPlan`;
  var command = { planId: planId, procedureId: procedureId };
  const response = await fetch(url, {
    method: "POST",
    headers: postHeaders,
    body: JSON.stringify(command),
  });

  if (!response.ok) throw new Error(StartPlanError);

  const responseData = await response.json();
  return responseData;
};

export const getProcedures = async (signal = null) => {
  const url = `${api_url}/Procedures`;
  const response = await fetch(url, {
    method: "GET",
    signal,
  });

  if (!response.ok) throw new Error(GetProceduresError);

  return await response.json();
};

export const getPlanProcedures = async (planId, signal = null) => {
  const url = `${api_url}/PlanProcedure?$filter=planId eq ${planId}&$expand=procedure`;
  const response = await fetch(url, {
    method: "GET",
    signal,
  });

  if (!response.ok) throw new Error(GetPlanProcedures);

  return await response.json();
};

export const getUsers = async (signal = null) => {
  const url = `${api_url}/Users`;
  const response = await fetch(url, {
    method: "GET",
    signal,
  });

  if (!response.ok) throw new Error(GetUsersError);

  return await response.json();
};

export const getUserAssignments = async (planProcedureId, signal = null) => {
  const url = `${api_url}/Users/GetUserAssignments?$filter=planProcedureId eq ${planProcedureId} and isDelete eq false`;
  const response = await fetch(url, {
    method: "GET",
    signal,
  });

  if (!response.ok) throw new Error(GetUserAssignmentsError);

  return await response.json();
};

export const addUserToProcedure = async (
  planProcedureId,
  userIds,
  signal = null
) => {
  const url = `${api_url}/Users/AddUserToProcedure`;
  var command = {
    planProcedureId: planProcedureId,
    userIds: userIds,
  };
  const response = await fetch(url, {
    method: "POST",
    headers: postHeaders,
    body: JSON.stringify(command),
    signal,
  });

  if (!response.ok) throw new Error(AddUserToProcedureError);

  return true;
};

export const removeUserFromProcedure = async (
  planProcedureId,
  userIds,
  signal = null
) => {
  const url = `${api_url}/Users/RemoveUserFromProcedure`;
  var command = {
    planProcedureId: planProcedureId,
    userIds: userIds,
  };
  const response = await fetch(url, {
    method: "DELETE",
    headers: postHeaders,
    body: JSON.stringify(command),
    signal,
  });

  if (!response.ok) throw new Error(AddUserToProcedureError);

  return true;
};
