// <copyright file="EventListing.ascx.cs" company="Engage Software">
// Engage: Events - http://www.engagemodules.com
// Copyright (c) 2004-2008
// by Engage Software ( http://www.engagesoftware.com )
// </copyright>
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

namespace Engage.Dnn.Events.Display
{
    using System;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using Engage.Events;
    using Templating;

    /// <summary>
    /// Custom event listing item
    /// </summary>
    public partial class EventListingItem : ModuleBase
    {
        private TemplateEngine engine;
        private Event currentEvent;
        private string headerTemplateName = string.Empty;
        private int pageSize = 1; //get from settings later

        //There are certain controls that wee need to maintain a reference to so that we can hide/show based on certain conditions i.e. ConfigurePager()
        private SortStatusAction sortStatusAction;
        private SortAction sortAction;
        private LinkButton NextButton;
        private LinkButton PreviousButton;
        private Label PageCountLabel;
        private Label CurrentPageLabel;
        private Label PageLabel;
        private Label OfLabel;
        
        
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            engine = new TemplateEngine(TabModuleId);
            RepeaterEvents.ItemDataBound += this.RepeaterEvents_ItemDataBound;
            this.LocalResourceFile = "~" + DesktopModuleFolderName + "Display/App_LocalResources/EventListingItem.ascx.resx";
            //this must be done here so all header/footer controls exist and viewstate restored i.e. sorting radio buttons, paging
            ProcessHeader();
            ProcessFooter();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                this.BindData();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the Listing control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void RepeaterEvents_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            try
            {
                this.currentEvent = (Event)e.Item.DataItem;

                if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
                {
                    Label labelId = (Label)e.Item.FindControl("LabelId");
                    if (labelId != null) labelId.Text = this.currentEvent.Id.ToString();
                    Template itemTemplate = TemplateEngine.GetTemplate(Server.MapPath("~/DesktopModules/EngageEvents/Templates/"), "ListingItem.html");
                    e.Item.Controls.Clear();
                   TemplateEngine.ProcessTags(e.Item, itemTemplate.ChildTags, ProcessTag);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        /// Processes the header.
        /// </summary>
        private void ProcessHeader()
        {
            PlaceHolderHeader.Controls.Clear();
            Template headerTemplate = this.headerTemplateName.Length == 0 ? TemplateEngine.GetTemplate(Server.MapPath("~/DesktopModules/EngageEvents/Templates/"), "ListingHeader.html") : TemplateEngine.GetTemplate(Server.MapPath("~/DesktopModules/EngageEvents/Templates/"), this.headerTemplateName);
            TemplateEngine.ProcessTags(PlaceHolderHeader, headerTemplate.ChildTags, this.ProcessTag);
        }

        /// <summary>
        /// Processes the footer.
        /// </summary>
        private void ProcessFooter()
        {
            PlaceHolderFooter.Controls.Clear();
            Template footerTemplate = TemplateEngine.GetTemplate(Server.MapPath("~/DesktopModules/EngageEvents/Templates/"), "ListingFooter.html");
            TemplateEngine.ProcessTags(PlaceHolderFooter,footerTemplate.ChildTags, this.ProcessTag);
        }

        /// <summary>
        /// Binds the data.
        /// </summary>
        internal void BindData()
        {
            string sort = "EventStart";
            if (this.sortAction != null)
            {
                sort = this.sortAction.SelectedValue;
            }

            string statusSort = "Active";
            if (this.sortStatusAction != null)
            {
                statusSort = this.sortStatusAction.SelectedValue;
            }

            //need to make sure that paging got rendered. if not, get all records
            if (this.PageCountLabel == null || this.CurrentPageLabel == null)
                this.pageSize = 0;

            EventCollection events = EventCollection.Load(PortalId, this.mode, sort, this.CurrentPageIndex , this.pageSize, statusSort == "All");
            RepeaterEvents.DataSource = events;
            RepeaterEvents.DataBind();

            ConfigurePager();
        }

        /// <summary>
        /// Configures the pager.
        /// </summary>
        private void ConfigurePager()
        {
            if (PageCountLabel != null) PageCountLabel.Text = PageCount.ToString();
            //Note: Because of viestate, we must check for null first and always turn on/off based on data.
            if (TotalRecords == 0)
            {
                if (PreviousButton != null) PreviousButton.Visible = false;
                if (NextButton != null) NextButton.Visible = false;
                if (PageCountLabel != null) PageCountLabel.Visible = false;
                if (CurrentPageLabel != null) CurrentPageLabel.Visible = false;
                if (PageLabel != null) PageLabel.Visible = false;
                if (OfLabel != null) OfLabel.Visible = false;
            }
            else
            {
                if (PreviousButton != null) PreviousButton.Visible = true;
                if (NextButton != null) NextButton.Visible = true;
                if (PageCountLabel != null) PageCountLabel.Visible = true;
                if (CurrentPageLabel != null) CurrentPageLabel.Visible = true;
                if (PageLabel != null) PageLabel.Visible = true;
                if (OfLabel != null) OfLabel.Visible = true;
            }
            if (TotalRecords == 1)
            {
                if (PreviousButton != null) PreviousButton.Visible = false;
                if (NextButton != null) NextButton.Visible = false;
            }

            if (PreviousButton != null) PreviousButton.Visible = (this.CurrentPageIndex != 0);
            if (NextButton != null) NextButton.Visible = (this.CurrentPageIndex + 1 < TotalRecords);
            if (CurrentPageLabel != null) CurrentPageLabel.Text = (CurrentPageIndex + 1).ToString();            
        }

        private ListingMode mode;
        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>The mode.</value>
        public ListingMode Mode
        {
            get { return this.mode; }
            set { this.mode = value; }

        }

        /// <summary>
        /// Method used to process a token. This method is invoked from the TemplateEngine class. Since this control knows
        /// best on how to contruct the page. ListingHeader, ListingItem and Listing Footer templates are processed here.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        private void ProcessTag(Control container, Tag tag)
        {
            EventCollection events = (EventCollection)RepeaterEvents.DataSource;
            string href;
            string resourceKey;

            switch (tag.LocalName.ToUpper())
            {
                case "EVENTTITLE":
                    Literal literalTitle = new Literal();
                    literalTitle.Text = this.currentEvent.Title;
                    container.Controls.Add(literalTitle);
                    break;
                case "EVENTDATE":
                    Literal literalWhen = new Literal();
                    string format = tag.Attributes[0].Value;
                    literalWhen.Text = this.currentEvent.EventStart.ToString(format);
                    container.Controls.Add(literalWhen);
                    break;
                case "EVENTMONTHSHORT":
                    Literal literalMonth = new Literal();
                    literalMonth.Text = this.currentEvent.EventStart.ToString("MMM");
                    container.Controls.Add(literalMonth);
                    break;
                case "EVENTMONTHLONG":
                    Literal literalMonthLong = new Literal();
                    literalMonthLong.Text = this.currentEvent.EventStart.ToString("MMMM");
                    container.Controls.Add(literalMonthLong);
                    break;
                case "EVENTDAYSHORT":
                    Literal literalDay = new Literal();
                    literalDay.Text = this.currentEvent.EventStart.ToString("%d");
                    container.Controls.Add(literalDay);
                    break;
                case "EVENTDAYLONG":
                    Literal literalDayLong = new Literal();
                    literalDayLong.Text = this.currentEvent.EventStart.ToString("dddd");
                    container.Controls.Add(literalDayLong);
                    break;
                case "EVENTYEARSHORT":
                    Literal literalYear = new Literal();
                    literalYear.Text = this.currentEvent.EventStart.ToString("yy");
                    container.Controls.Add(literalYear);
                    break;
                case "EVENTYEARLONG":
                    Literal literalYearLong = new Literal();
                    literalYearLong.Text = this.currentEvent.EventStart.ToString("yyyy");
                    container.Controls.Add(literalYearLong);
                    break;
                case "EVENTLOCATION":
                    Literal literalLocation = new Literal();
                    literalLocation.Text = this.currentEvent.Location;
                    container.Controls.Add(literalLocation);
                    break;
                case "EVENTDESCRIPTION":
                    Literal literalLocationDescription = new Literal();
                    literalLocationDescription.Text = this.currentEvent.Overview;
                    container.Controls.Add(literalLocationDescription);
                    break;
                case "EDITEVENTBUTTON":
                    ButtonAction editEventAction = (ButtonAction)LoadControl("~" + DesktopModuleFolderName + "Actions/ButtonAction.ascx");
                    editEventAction.CurrentEvent = this.currentEvent;
                    editEventAction.ModuleConfiguration = ModuleConfiguration;
                    href = BuildLinkUrl("&modId=" + ModuleId.ToString(CultureInfo.InvariantCulture) + "&key=EventEdit&eventId=" + this.currentEvent.Id.ToString(CultureInfo.InvariantCulture));
                    editEventAction.Href = href;
                    editEventAction.Text = Localization.GetString("EditEventButton", "~" + DesktopModuleFolderName + "Navigation/App_LocalResources/EventAdminActions");
                    container.Controls.Add(editEventAction);
                    break;
                case "VIEWRESPONSESBUTTON":
                    ButtonAction responsesEventAction = (ButtonAction)LoadControl("~" + DesktopModuleFolderName + "Actions/ButtonAction.ascx");
                    responsesEventAction.CurrentEvent = this.currentEvent;
                    responsesEventAction.ModuleConfiguration = ModuleConfiguration;
                    href = BuildLinkUrl("&modId=" + ModuleId.ToString(CultureInfo.InvariantCulture) + "&key=RsvpDetail&eventid=" + this.currentEvent.Id.ToString(CultureInfo.InvariantCulture));
                    responsesEventAction.Href = href;
                    responsesEventAction.Text = Localization.GetString("ResponsesButton", "~" + DesktopModuleFolderName + "Navigation/App_LocalResources/EventAdminActions");
                    container.Controls.Add(responsesEventAction);
                    break;
                case "REGISTERBUTTON":
                    ButtonAction registerEventAction = (ButtonAction)LoadControl("~" + DesktopModuleFolderName + "Actions/ButtonAction.ascx");
                    registerEventAction.CurrentEvent = this.currentEvent;
                    registerEventAction.ModuleConfiguration = ModuleConfiguration;
                    href = BuildLinkUrl("&modId=" + ModuleId.ToString(CultureInfo.InvariantCulture) + "&key=Register&eventid=" + this.currentEvent.Id.ToString(CultureInfo.InvariantCulture));
                    registerEventAction.Href = href;
                    registerEventAction.Text = Localization.GetString("RegisterButton", "~" + DesktopModuleFolderName + "Navigation/App_LocalResources/EventAdminActions");
                    container.Controls.Add(registerEventAction);
                    break;
                case "ADDTOCALENDARBUTTON":
                    AddToCalendarAction addToCalendarAction = (AddToCalendarAction)LoadControl("~" + DesktopModuleFolderName + "Actions/AddToCalendarAction.ascx");
                    addToCalendarAction.CurrentEvent = this.currentEvent;
                    addToCalendarAction.ModuleConfiguration = ModuleConfiguration;
                    container.Controls.Add(addToCalendarAction);
                    break;
                case "DELETEBUTTON":
                    DeleteAction deleteAction = (DeleteAction)LoadControl("~" + DesktopModuleFolderName + "Actions/DeleteAction.ascx");
                    deleteAction.CurrentEvent = this.currentEvent;
                    deleteAction.ModuleConfiguration = ModuleConfiguration;
                    container.Controls.Add(deleteAction);
                    break;
                case "CANCELBUTTON":
                    CancelAction cancelAction = (CancelAction)LoadControl("~" + DesktopModuleFolderName + "Actions/CancelAction.ascx");
                    cancelAction.CurrentEvent = this.currentEvent;
                    cancelAction.ModuleConfiguration = ModuleConfiguration;
                    container.Controls.Add(cancelAction);
                    break;
                case "EDITEMAILBUTTON":
                    ButtonAction editEmailAction = (ButtonAction)LoadControl("~" + DesktopModuleFolderName + "Actions/ButtonAction.ascx");
                    editEmailAction.CurrentEvent = this.currentEvent;
                    editEmailAction.ModuleConfiguration = ModuleConfiguration;
                    href = BuildLinkUrl("&modId=" + ModuleId.ToString(CultureInfo.InvariantCulture) + "&key=EmailEdit&eventid=" + this.currentEvent.Id.ToString(CultureInfo.InvariantCulture));
                    editEmailAction.Href = href;
                    editEmailAction.Text = Localization.GetString("EditEmailButton", "~" + DesktopModuleFolderName + "Navigation/App_LocalResources/EventAdminActions");
                    container.Controls.Add(editEmailAction);
                    break;
                case "SORTEVENTBYDATE":
                    this.sortAction = (SortAction)LoadControl("~" + DesktopModuleFolderName + "Actions/SortAction.ascx");
                    sortAction.ModuleConfiguration = ModuleConfiguration;
                    sortAction.SortChanged += this.sortStatusAction_SortChanged;
                    container.Controls.Add(sortAction);
                    this.sortAction = sortAction;
                    break;
                case "SORTEVENTBYSTATUS":
                    this.sortStatusAction = (SortStatusAction)LoadControl("~" + DesktopModuleFolderName + "Actions/SortStatusAction.ascx");
                    sortStatusAction.ModuleConfiguration = ModuleConfiguration;
                    sortStatusAction.SortChanged += this.sortStatusAction_SortChanged;
                    container.Controls.Add(sortStatusAction);
                    this.sortStatusAction = sortStatusAction;
                    break;
                case "PREVIOUSPAGE":
                    this.PreviousButton = new LinkButton();
                    this.PreviousButton.Text = Localization.GetString("PreviousButton", LocalResourceFile);
                    this.PreviousButton.CssClass = tag.GetAttributeValue("CssClass");
                    this.PreviousButton.CommandName = "PreviousPage";
                    this.PreviousButton.EnableViewState = true;
                    this.PreviousButton.ToolTip = Localization.GetString(tag.GetAttributeValue("ToolTipResourceKey"), LocalResourceFile);

                    //if (CurrentPageIndex == 0)
                    //{
                    //    PreviousButton.Enabled = false;
                    //}

                    this.PreviousButton.Click += PreviousButton_Click;
                    container.Controls.Add(this.PreviousButton);
                    break;
                case "NEXTPAGE":
                    this.NextButton = new LinkButton();
                    this.NextButton.Text = Localization.GetString("NextButton", LocalResourceFile);
                    this.NextButton.CssClass = tag.GetAttributeValue("CssClass");
                    this.NextButton.CommandName = "NextPage";
                    this.NextButton.EnableViewState = true;
                    this.NextButton.ToolTip = Localization.GetString(tag.GetAttributeValue("ToolTipResourceKey"), LocalResourceFile);
                    this.NextButton.Click += NextButton_Click;
                    container.Controls.Add(this.NextButton);
                    break;
                case "PAGER":
                    //int cp;
                    //string fs = Localization.GetString("Pager", LocalResourceFile);
                    //DropDownList objDropDown = new DropDownList();
                    //objDropDown.ID = "lnkPgHPages";
                    //objDropDown.CssClass = oRepositoryBusinessController.GetSkinAttribute(xmlHeaderDoc, "PAGER", "CssClass", "normal");
                    //objDropDown.Width = System.Web.UI.WebControls.Unit.Parse(oRepositoryBusinessController.GetSkinAttribute(xmlHeaderDoc, "PAGER", "Width", "75"));
                    //objDropDown.AutoPostBack = true;
                    //cp = 1;
                    //while (cp < lstObjects.PageCount + 1)
                    //{
                    //    objdropdown.Items.Add(new ListItem(string.Format(fs, cp), cp));
                    //    cp = cp + 1;
                    //}

                    //objdropdown.SelectedValue = lstObjects.CurrentPageIndex + 1;
                    //objDropDown.SelectedIndexChanged += lnkPg_SelectedIndexChanged;
                    //hPlaceHolder.Controls.Add(objDropDown);
                    break;
                case "CURRENTPAGE":
                    this.CurrentPageLabel = new Label();
                    CurrentPageLabel.Text = (CurrentPageIndex + 1).ToString();
                    CurrentPageLabel.CssClass = tag.GetAttributeValue("CssClass");
                    CurrentPageLabel.ToolTip = Localization.GetString("CurrentPageToolTip", LocalResourceFile);
                    container.Controls.Add(CurrentPageLabel);
                    break;
                case "PAGECOUNT":
                    this.PageCountLabel = new Label();
                    this.PageCountLabel.CssClass = tag.GetAttributeValue("CssClass");
                    container.Controls.Add(this.PageCountLabel);
                    break;
                case "PAGELABEL":
                    this.PageLabel = new Label();
                    resourceKey = tag.GetAttributeValue("ResourceKey");
                    PageLabel.Text = Localization.GetString(resourceKey, LocalResourceFile);
                    PageLabel.CssClass = tag.GetAttributeValue("CssClass");
                    container.Controls.Add(PageLabel);
                    break;
                case "OFLABEL":
                    this.OfLabel = new Label();
                    resourceKey = tag.GetAttributeValue("ResourceKey");
                    OfLabel.Text = Localization.GetString(resourceKey, LocalResourceFile);
                    OfLabel.CssClass = tag.GetAttributeValue("CssClass");
                    container.Controls.Add(OfLabel);
                    break;
                case "LABEL":
                    Label l = new Label();
                    resourceKey = tag.GetAttributeValue("ResourceKey");
                    l.Text = Localization.GetString(resourceKey, LocalResourceFile);
                    l.CssClass = tag.GetAttributeValue("CssClass");
                    container.Controls.Add(l);
                    break;
                default:
                    break;
            }

        }

        private void sortStatusAction_SortChanged(object sender, EventArgs e)
        {
            CurrentPageIndex = 0;
            ConfigurePager();
        }

        /// <summary>
        /// Gets or sets the index of the current page. Zero based index.
        /// </summary>
        /// <value>The index of the current page.</value>
        private int CurrentPageIndex
        {
            get { return Convert.ToInt32(ViewState["CurrentPageIndex"]);}
            set {ViewState["CurrentPageIndex"] = value;}
        }

        /// <summary>
        /// Calculates the number of pages.
        /// </summary>
        /// <value>The page count.</value>
        private int PageCount
        {
            get
            {
                int count;
                if (TotalRecords > pageSize)
                {
                    count = (TotalRecords / pageSize);
                }
                else
                {
                    count = 1;
                }
                return count;
            }
        }

        /// <summary>
        /// Gets the total records in the request to the database. NOTE: not events.count
        /// </summary>
        /// <value>The total records.</value>
        private int TotalRecords
        {
            get
            {
                EventCollection events = (EventCollection) RepeaterEvents.DataSource;
                return events.TotalRecords;
            }
        }

        /// <summary>
        /// Handles the Click event of the NextButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void NextButton_Click(object sender, EventArgs e)
        {
            int pageNumber;
            try
            {
                pageNumber = CurrentPageIndex + 1;
            }
            catch (Exception ex)
            {
                pageNumber = CurrentPageIndex;
            }
            if (pageNumber > PageCount - 1)
            {
                pageNumber = PageCount - 1;
            }
            CurrentPageIndex = pageNumber;
            CurrentPageLabel.Text = CurrentPageIndex.ToString();
            BindData();
        }

        /// <summary>
        /// Handles the Click event of the PreviousButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void PreviousButton_Click(object sender, EventArgs e)
        {
            int pageNumber;
            try
            {
                pageNumber = CurrentPageIndex - 1;
            }
            catch (Exception ex)
            {
                pageNumber = CurrentPageIndex;
            }
            if (pageNumber < 0)
            {
                pageNumber = 0;
            }
            CurrentPageIndex = pageNumber;
            BindData();
        }

        /// <summary>
        /// Sets the name of the header template. Can be specified in the Listing.html as an attribute: HeaderTemplate="..."
        /// </summary>
        /// <value>The name of the header template.</value>
        internal string HeaderTemplateName
        {
            set { this.headerTemplateName = value; }   
        }
    }
}

