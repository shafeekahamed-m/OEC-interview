import React from "react";
import "@testing-library/jest-dom";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import * as apiModule from "../api/api"; // Import the entire module
import PlanProcedureItem from "../components/Plan/PlanProcedureItem/PlanProcedureItem";

// Mocking the API functions
jest.mock("../api/api");

const mockProcedure = {
  procedureTitle: "Test Procedure",
};

const mockUsers = [
  { label: "User1", value: 1 },
  { label: "User2", value: 2 },
  // Add more mock users as needed
];

const planProcedureId = 123;

describe("PlanProcedureItem Component", () => {
    it('renders the component with the procedure title', () => {
      render(<PlanProcedureItem procedure={mockProcedure} users={mockUsers} planProcedureId={planProcedureId} />);

      const procedureTitleElement = screen.getByText("Test Procedure");
      expect(procedureTitleElement).toBeInTheDocument();
    });

    it('renders the multi-select dropdown with users', async () => {
      render(<PlanProcedureItem procedure={mockProcedure} users={mockUsers} planProcedureId={planProcedureId} />);

      //Selecting by role
      const selectDropdown = screen.getByRole('combobox');
      userEvent.click(selectDropdown);

      // Wait for the dropdown options to appear
      await waitFor(() => {
        const userOption1 = screen.getByText('User1');
        const userOption2 = screen.getByText('User2');

        expect(userOption1).toBeInTheDocument();
        expect(userOption2).toBeInTheDocument();
      });
    });

  it("assigns users to the procedure and calls the API functions", async () => {
    const addUserToProcedureMock = jest.fn();
    const getUserAssignmentsMock = jest.fn(() =>
      Promise.resolve([{ userId: 1 }])
    );

    // Assign module to a variable
    const apiMock = apiModule;

    // Mocking the API functions with custom implementations
    apiMock.addUserToProcedure = addUserToProcedureMock;
    apiMock.getUserAssignments = getUserAssignmentsMock;

    render(
      <PlanProcedureItem
        procedure={mockProcedure}
        users={mockUsers}
        planProcedureId={planProcedureId}
      />
    );

    const selectDropdown = screen.getByRole("combobox");

    //waiting for the drop down to be loaded and selected and then execute the mock api's
    await waitFor(() => {
      userEvent.click(selectDropdown);

      // Selecting users from the dropdown
      userEvent.click(screen.getByText("User1"));
      userEvent.click(screen.getByText("User2"));

      // Triggering the change event and waiting for the API calls
      userEvent.click(selectDropdown);
    });

    // Validating that the API functions are called with the correct parameters
    await waitFor(() => {
      expect(addUserToProcedureMock).toHaveBeenNthCalledWith(1, 123, [1]);
      expect(getUserAssignmentsMock).toHaveBeenCalledWith(
        123,
        expect.anything()
      );
      expect(addUserToProcedureMock).toHaveBeenNthCalledWith(2, 123, [2]);
      expect(getUserAssignmentsMock).toHaveBeenCalledWith(
        123,
        expect.anything()
      );
    });
  });
});
