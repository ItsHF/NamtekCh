import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

const TimesheetList = () => {
  const [timesheets, setTimesheets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Fetch timesheets when the component mounts
  useEffect(() => {
    fetch('https://localhost:7035/api/Timesheets')
      .then(response => response.json())
      .then(data => {
        setTimesheets(data);
        setLoading(false); // Stop loading after data is fetched
      })
      .catch(err => {
        setError('Failed to load timesheets');
        setLoading(false); // Stop loading even on error
        console.error('Error fetching timesheets:', err);
      });
  }, []);

  // Function to format dates
  const formatDate = (dateString) => {
    const options = { year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' };
    const date = new Date(dateString);
    return date.toLocaleString('en-US', options);
  };

  if (loading) {
    return <div>Loading...</div>; // Show loading message or spinner
  }

  return (
    <div>
      <h1>Timesheet List</h1>
      {error && <div style={{ color: 'red' }}>{error}</div>} {/* Show error message if any */}
      
      <table>
        <thead>
          <tr>
            <th>Employee</th>
            <th>Start Time</th>
            <th>End Time</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {timesheets.map(timesheet => (
            <tr key={timesheet.id}>
              <td>{timesheet.employee.name}</td>
              <td>{formatDate(timesheet.startTime)}</td>
              <td>{formatDate(timesheet.endTime)}</td>
              <td>
                <Link to={`/Timesheets/${timesheet.id}`}>View</Link>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      
      <Link to="/Timesheets/new">Add New Timesheet</Link>
    </div>
  );
};

export default TimesheetList;
