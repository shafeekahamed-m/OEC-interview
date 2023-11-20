const api_url = process.env.REACT_APP_API_BASE_URL;

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

  if (!response.ok) throw new Error("Failed to create plan");

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

  if (!response.ok) throw new Error("Failed to create plan");

  const responseData = await response.json();
  return responseData;
};

export const getProcedures = async (signal) => {
  const url = `${api_url}/Procedures`;
  const response = await fetch(url, {
    method: "GET",
    signal,
  });

  if (!response.ok) throw new Error("Failed to get procedures");

  return await response.json();
};

export const getPlanProcedures = async (planId, signal) => {
  const url = `${api_url}/PlanProcedure?$filter=planId eq ${planId}&$expand=procedure`;
  const response = await fetch(url, {
    method: "GET",
    signal,
  });

  if (!response.ok) throw new Error("Failed to get plan procedures");

  return await response.json();
};

export const getUsers = async (signal) => {
  const url = `${api_url}/Users`;
  const response = await fetch(url, {
    method: "GET",
    signal,
  });

  if (!response.ok) throw new Error("Failed to get users");

  return await response.json();
};

export const getUserAssignments = async (planProcedureId, signal) => {
  const url = `${api_url}/Users/GetUserAssignments?$filter=planProcedureId eq ${planProcedureId} and isDelete eq false`;
  const response = await fetch(url, {
    method: "GET",
    signal,
  });

  if (!response.ok) throw new Error("Failed to get plan procedures");

  return await response.json();
};

export const addUserToProcedure = async (planProcedureId, userIds, signal) => {
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

  if (!response.ok) throw new Error("Failed to create plan");

  return true;
};
